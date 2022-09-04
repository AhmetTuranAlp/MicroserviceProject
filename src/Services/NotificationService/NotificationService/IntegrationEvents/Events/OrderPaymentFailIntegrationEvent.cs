using EventBus.Base.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotificationService.IntegrationEvents.Events
{
    public class OrderPaymentFailIntegrationEvent: IntegrationEvent
    {
        public int OrderId { get; set; }
        public string ErrorMessage { get; set; }

        public OrderPaymentFailIntegrationEvent(int orderId, string errorMessage)
        {
            OrderId = orderId;
            ErrorMessage = errorMessage;
        }
    }
}
