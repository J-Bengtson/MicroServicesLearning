using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Core.Events;

namespace NotificationService.Consumers
{
    public class UserCreatedEmailConsumer : IConsumer<UserCreatedIntegrationEvent>
    {
        private readonly ILogger<UserCreatedEmailConsumer> _logger;

        public UserCreatedEmailConsumer(ILogger<UserCreatedEmailConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            
            // Simulating an email being sent
            _logger.LogInformation("========================================");
            _logger.LogInformation($"[NOTIFICATION SERVICE] Sending Welcome Email!");
            _logger.LogInformation($"To: {message.Email}");
            _logger.LogInformation($"Subject: Welcome to our platform, {message.Username}!");
            _logger.LogInformation("========================================");

            await Task.CompletedTask;
        }
    }
}
