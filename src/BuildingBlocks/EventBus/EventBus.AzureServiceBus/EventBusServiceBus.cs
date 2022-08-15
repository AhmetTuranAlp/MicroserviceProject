using EventBus.Base;
using EventBus.Base.Events;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus.AzureServiceBus
{
    public class EventBusServiceBus : BaseEventBus
    {
        private ITopicClient topicClient;
        private ManagementClient managementClient;
        private ILogger logger;

        public EventBusServiceBus(IServiceProvider serviceProvider, EventBusConfig eventBusConfig) : base(serviceProvider, eventBusConfig)
        {
            logger = serviceProvider.GetService(typeof(EventBusServiceBus)) as ILogger<EventBusServiceBus>;
            managementClient = new ManagementClient(eventBusConfig.EventBusConnectionString);
            topicClient = CreateTopicClient();
        }

        /// <summary>
        /// Topic kontrol işlemi yapılmaktadır.
        /// </summary>
        /// <returns></returns>
        private ITopicClient CreateTopicClient()
        {
            if (topicClient == null || topicClient.IsClosedOrClosing)
                topicClient = new TopicClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, RetryPolicy.Default);

            //Topic control and topic create operation
            if (!managementClient.TopicExistsAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult())
                managementClient.CreateTopicAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult();

            return topicClient;
        }

        /// <summary>
        /// AzureServiceBus Gönderimi Yapılmaktadır.
        /// </summary>
        /// <param name="event"></param>
        public override void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;//example: OrderCreatedIntegrationEvent
            eventName = ProcessEventName(eventName);//example: OrderCreated
            var eventStr = JsonConvert.SerializeObject(@event);
            var bodyArr = Encoding.UTF8.GetBytes(eventStr);

            var message = new Message()
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = bodyArr,
                Label = eventName
            };

            topicClient.SendAsync(message).GetAwaiter().GetResult();
        }

        public override void Subscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);

            if (!subsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptionClient = CreateSubscriptionClientIfNotExists(eventName);
                RegisterSubscriptionClientMessageHandler(subscriptionClient);
            }

            //logger.LogInformation("Subscription to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

            subsManager.AddSubscription<T, TH>();
        }

        public override void UnSubscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            try
            {
                var subscriptionClient = CreateSubscriptionClient(eventName);
                subscriptionClient.RemoveRuleAsync(eventName).GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                //logger.LogWarning("The messaging entity {eventNama} Could not be found.", eventName);
            }
            //logger.LogInformation("UnSubscribing from event {EventName}", eventName);
            subsManager.RemoveSubscription<T, TH>();
        }

        private void RegisterSubscriptionClientMessageHandler(ISubscriptionClient subscriptionClient)
        {
            subscriptionClient.RegisterMessageHandler(
                async (message, token) =>
                {
                    var eventName = $"{message.Label}";
                    var messageData = Encoding.UTF8.GetString(message.Body);

                    //Mesaj parçalama işlemi yapılıp ilgili servislere gönderilmesi yapılmaktadır. Sonrasında ise complete ile beraber başka bir işleme gönderilmemesi için tamamlandı olarak işlem yapılmaktadır.
                    if (await ProcessEvent(ProcessEventName(eventName), messageData))
                    {
                        await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                    }
                },
                //Mesaj tüketme aşamasında bir hata alınması durumunda işleme girmektedir.
                new MessageHandlerOptions(ExemptionRecivedHandler) { MaxConcurrentCalls = 10, AutoComplete = false });

        }

        private Task ExemptionRecivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex = exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            //logger.LogError(ex, "Error handling message: {ExemptionMessage} - Content: {@ExemptionContext}", ex.Message, context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Subscription oluşturma işlemi yapılmaktadır.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        private ISubscriptionClient CreateSubscriptionClientIfNotExists(string eventName)
        {
            var subClient = CreateSubscriptionClient(eventName);
            var exists = managementClient.SubscriptionExistsAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
            if (!exists)
            {
                managementClient.CreateSubscriptionAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
                RemoveDefaultRule(subClient);
            }
            CreateRuleIfNotExists(ProcessEventName(eventName), subClient);
            return subClient;
        }

        /// <summary>
        /// Rule olup olmadıgı kontrol edilmektedir. Yok ise oluşturulmaktadır.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="subscriptionClient"></param>
        private void CreateRuleIfNotExists(string eventName, ISubscriptionClient subscriptionClient)
        {
            bool ruleExists;
            try
            {
                var rule = managementClient.GetRuleAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName), eventName).GetAwaiter().GetResult();
                ruleExists = rule != null;
            }
            catch (MessagingEntityNotFoundException ex)
            {
                ruleExists = false;
            }

            if (!ruleExists)
            {
                subscriptionClient.AddRuleAsync(new RuleDescription
                {
                    Filter = new CorrelationFilter { Label = eventName },
                    Name = eventName
                }).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Default olan rule için silme işlemi gerçekleştirir.
        /// </summary>
        /// <param name="subscriptionClient"></param>
        private void RemoveDefaultRule(SubscriptionClient subscriptionClient)
        {
            try
            {
                subscriptionClient.RemoveRuleAsync(RuleDescription.DefaultRuleName).GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {
                //logger.LogWarning("The messaging entity {DefaultRuleName} Could not be found.", RuleDescription.DefaultRuleName);
            }
        }

        /// <summary>
        /// EventName ile beraber subscription elde edilmektedir.
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        private SubscriptionClient CreateSubscriptionClient(string eventName)
        {
            return new SubscriptionClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, GetSubName(eventName));
        }

        public override void Dispose()
        {
            base.Dispose();

            topicClient.CloseAsync().GetAwaiter().GetResult();
            managementClient.CloseAsync().GetAwaiter().GetResult();
            topicClient = null;
            managementClient = null;
        }
    }
}
