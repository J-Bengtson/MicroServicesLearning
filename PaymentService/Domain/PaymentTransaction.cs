using System;

namespace PaymentService.Domain
{
    public class PaymentTransaction
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime ProcessedAt { get; private set; } = DateTime.UtcNow;
        public bool IsSuccessful { get; private set; } = true;
        public string? FailureReason { get; private set; }

        private PaymentTransaction() { } // For EF Core

        public static PaymentTransaction Create(Guid userId, decimal amount)
        {
            var transaction = new PaymentTransaction
            {
                UserId = userId,
                Amount = amount
            };

            if (amount <= 0)
            {
                transaction.FailPayment("Invalid payment amount. Amount must be greater than zero.");
            }

            return transaction;
        }

        public void FailPayment(string reason)
        {
            IsSuccessful = false;
            FailureReason = reason;
        }
    }
}
