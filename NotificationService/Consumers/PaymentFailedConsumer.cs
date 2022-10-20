using System;
using System.Threading.Tasks;
using MassTransit;
using Core.Events;
using Microsoft.Extensions.Logging;

namespace NotificationService.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailedIntegrationEvent>
    {
        private readonly ILogger<PaymentFailedConsumer> _logger;

        public PaymentFailedConsumer(ILogger<PaymentFailedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<PaymentFailedIntegrationEvent> context)
        {
            var failedEvent = context.Message;
            
            _logger.LogWarning("\n===================================\n" +
                               "❌ PAYMENT FAILED ALERT\n" +
                               "To: User {UserId}\n" +
                               "Amount Attempted: ${Amount}\n" +
                               "Time: {FailedAt}\n" +
                               "Reason: {Reason}\n" +
                               "Message: We couldn't process your payment. Please check your payment method and try again.\n" +
                               "===================================\n", 
                               failedEvent.UserId, failedEvent.Amount, failedEvent.FailedAt, failedEvent.Reason);

            return Task.CompletedTask;
        }
    }
}
