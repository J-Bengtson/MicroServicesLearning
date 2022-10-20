using System.Threading.Tasks;
using MassTransit;
using Core.Events;
using Microsoft.Extensions.Logging;
using UserService.Infrastructure;

namespace UserService.Consumers
{
    public class PaymentProcessedConsumer : IConsumer<PaymentProcessedIntegrationEvent>
    {
        private readonly UserDbContext _dbContext;
        private readonly ILogger<PaymentProcessedConsumer> _logger;

        public PaymentProcessedConsumer(UserDbContext dbContext, ILogger<PaymentProcessedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentProcessedIntegrationEvent> context)
        {
            var paymentEvent = context.Message;
            
            if (!paymentEvent.IsSuccessful)
            {
                _logger.LogWarning("Payment {PaymentId} failed for user {UserId}. Skipping premium upgrade.", paymentEvent.PaymentId, paymentEvent.UserId);
                return;
            }

            var user = await _dbContext.Users.FindAsync(paymentEvent.UserId);
            if (user != null)
            {
                user.IsPremium = true;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("User {UserId} upgraded to Premium status successfully!", user.Id);
            }
            else
            {
                _logger.LogWarning("User {UserId} not found for premium upgrade.", paymentEvent.UserId);
            }
        }
    }
}
