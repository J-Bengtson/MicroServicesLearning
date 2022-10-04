using System;

namespace Core.Events
{
    public class PaymentProcessedIntegrationEvent
    {
        public Guid PaymentId { get; init; }
        public Guid UserId { get; init; }
        public decimal Amount { get; init; }
        public bool IsSuccessful { get; init; }
    }
}
