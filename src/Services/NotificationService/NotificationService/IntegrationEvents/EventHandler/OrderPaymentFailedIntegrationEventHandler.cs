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
    public class OrderPaymentFailedIntegrationEventHandler : IIntegrationEventHandler<OrderPaymentFailIntegrationEvent>
    {
        private readonly ILogger<OrderPaymentFailedIntegrationEventHandler> logger;

        public OrderPaymentFailedIntegrationEventHandler(ILogger<OrderPaymentFailedIntegrationEventHandler> logger)
        {
            this.logger = logger;
        }

        public Task Handle(OrderPaymentFailIntegrationEvent @event)
        {
            logger.LogInformation($"Order Payment failed with OrderId: {@event.OrderId} ErrorMessage: {@event.ErrorMessage}");
            return Task.CompletedTask;
        }
    }
}
