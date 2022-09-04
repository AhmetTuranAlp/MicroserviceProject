using EventBus.Base.Abstraction;
using Microsoft.Extensions.Logging;
using NotificationService.IntegrationEvents.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.IntegrationEvents.EventHandler
{
    public class OrderPaymentSuccessIntegrationEventHandlery : IIntegrationEventHandler<OrderPaymentSuccessIntegrationEvent>
    {
        private readonly ILogger<OrderPaymentSuccessIntegrationEvent> logger;

        public OrderPaymentSuccessIntegrationEventHandlery(ILogger<OrderPaymentSuccessIntegrationEvent> logger)
        {
            this.logger = logger;
        }

        public Task Handle(OrderPaymentSuccessIntegrationEvent @event)
        {
            logger.LogInformation($"Order Payment success with OrderId: {@event.OrderId}");
            return Task.CompletedTask;
        }
    }
}
