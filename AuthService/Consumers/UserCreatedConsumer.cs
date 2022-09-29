using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Core.Events;
using AuthService.Infrastructure;
using AuthService.Domain;

namespace AuthService.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreatedIntegrationEvent>
    {
        private readonly ILogger<UserCreatedConsumer> _logger;
        private readonly AuthDbContext _dbContext;

        public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger, AuthDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task Consume(ConsumeContext<UserCreatedIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received UserCreatedIntegrationEvent for User {UserId} ({Email})", message.UserId, message.Email);

            var log = new LogAuthentication
            {
                UserId = message.UserId,
                IsSuccessful = true,
                IpAddress = "System: RabbitMQ Event",
                UserAgent = "MassTransit Consumer"
            };

            _dbContext.LogAuthentications.Add(log);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("User {UserId} registered in Auth Database via Integration Event.", message.UserId);
        }
    }
}
