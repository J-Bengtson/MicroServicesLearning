using System;
using System.Threading.Tasks;
using MassTransit;
using Core.Events;
using Microsoft.Extensions.Logging;

namespace NotificationService.Consumers
{
    public class PaymentReceiptConsumer : IConsumer<PaymentProcessedIntegrationEvent>
    {
        private readonly ILogger<PaymentReceiptConsumer> _logger;

        public PaymentReceiptConsumer(ILogger<PaymentReceiptConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<PaymentProcessedIntegrationEvent> context)
        {
            var paymentEvent = context.Message;
            
            if (paymentEvent.IsSuccessful)
            {
                _logger.LogInformation("\n===================================\n" +
                                       "✉️ EMAIL SENT (PAYMENT RECEIPT)\n" +
                                       "To: User {UserId}\n" +
                                       "Amount: {Amount:C}\n" +
                                       "Message: Thank you for your purchase! You are now a Premium Member.\n" +
                                       "===================================\n", 
                                       paymentEvent.UserId, paymentEvent.Amount);
            }
            else
            {
                _logger.LogWarning("Payment {PaymentId} failed. Sending failure notification to user {UserId}.", paymentEvent.PaymentId, paymentEvent.UserId);
            }

            return Task.CompletedTask;
        }
    }
}
