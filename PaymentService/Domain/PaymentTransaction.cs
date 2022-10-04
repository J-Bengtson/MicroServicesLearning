using System;

namespace PaymentService.Domain
{
    public class PaymentTransaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
        public bool IsSuccessful { get; set; }
    }
}
