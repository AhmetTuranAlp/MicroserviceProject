using EventBus.Base;
using EventBus.Base.Abstraction;
using EventBus.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationService.IntegrationEvents.EventHandler;
using NotificationService.IntegrationEvents.Events;
using RabbitMQ.Client;
using System;

namespace NotificationService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();
            ConfigurationServices(services);

            var sp = services.BuildServiceProvider();
            IEventBus eventBus =  sp.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderPaymentSuccessIntegrationEvent, OrderPaymentSuccessIntegrationEventHandlery>();
            eventBus.Subscribe<OrderPaymentFailIntegrationEvent, OrderPaymentFailedIntegrationEventHandler>();


            Console.WriteLine("Hello World!");
        }

        private static void ConfigurationServices(ServiceCollection services)
        {
            services.AddLogging(configuration => configuration.AddConsole());

            services.AddTransient<OrderPaymentFailedIntegrationEventHandler>();
            services.AddTransient<OrderPaymentSuccessIntegrationEventHandlery>();

            services.AddTransient<IEventBus>(sp =>
            {
                EventBusConfig config = new()
                {
                    ConnectionRetryCount = 5,
                    EventNameSuffix = "IntegrationEvent",
                    SubscribeClientAppName = "NotificationService",
                    Connection = new ConnectionFactory(),
                    EventBusType = EventBusType.RabbitMQ
                };
                return EventBusFactory.Create(config, sp);
            });
        }
    }
}
