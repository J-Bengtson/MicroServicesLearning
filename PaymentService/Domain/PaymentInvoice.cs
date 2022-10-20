using System;

namespace PaymentService.Domain
{
    public class PaymentInvoice
    {
        public Guid Id { get; private set; }
        public Guid PaymentTransactionId { get; private set; }
        public DateTime IssueDate { get; private set; }
        public string DocumentUrl { get; private set; }

        private PaymentInvoice() { } // Para o EF Core

        public static PaymentInvoice Create(Guid transactionId)
        {
            return new PaymentInvoice
            {
                Id = Guid.NewGuid(),
                PaymentTransactionId = transactionId,
                IssueDate = DateTime.UtcNow,
                DocumentUrl = $"https://microservices-storage.local/invoices/{transactionId}.pdf" // Simulando um PDF gerado
            };
        }
    }
}
