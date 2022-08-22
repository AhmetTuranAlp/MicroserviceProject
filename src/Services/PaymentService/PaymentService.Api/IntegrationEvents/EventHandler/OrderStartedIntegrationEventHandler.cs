using EventBus.Base.Abstraction;
using EventBus.Base.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Api.IntegrationEvents.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Api.IntegrationEvents.EventHandler
{
    public class OrderStartedIntegrationEventHandler : IIntegrationEventHandler<OrderStartedIntegrationEvent>
    {
        private readonly IConfiguration configuration;
        private readonly IEventBus eventBus;
        private readonly ILogger<OrderStartedIntegrationEventHandler> logger;

        public OrderStartedIntegrationEventHandler(IConfiguration configuration, IEventBus eventBus, ILogger<OrderStartedIntegrationEventHandler> logger)
        {
            this.configuration = configuration;
            this.eventBus = eventBus;
            this.logger = logger;
        }

        public Task Handle(OrderStartedIntegrationEvent @event)
        {
            string keyword = "PaymentSuccess";
            bool paymentSuccessFlag = configuration.GetValue<bool>(keyword);
            IntegrationEvent paymentEvent = paymentSuccessFlag ? new OrderPaymentSuccessIntegrationEvent() : new OrderPaymentFailIntegrationEvent(@event.OrderId, "This is a fake error messege");

            logger.LogInformation($"OrderStartedIntegrationEventHandler is PaymentService is fired with PaymentSuccess: {paymentSuccessFlag}, OrderId: {@event.OrderId}");

            eventBus.Publish(paymentEvent);
            return Task.CompletedTask;

        }
    }
}
