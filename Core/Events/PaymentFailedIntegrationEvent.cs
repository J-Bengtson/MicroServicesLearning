using System;

namespace Core.Events
{
    public class PaymentFailedIntegrationEvent
    {
        public Guid PaymentId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime FailedAt { get; set; }
    }
}
