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
            
            var authUser = new AuthUser
            {
                Id = message.UserId,
                Email = message.Email,
                PasswordHash = message.PasswordHash
            };

            _dbContext.AuthUsers.Add(authUser);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation($"[AUTH SERVICE] User replicated: {message.UserId} / {message.Email}");
        }
    }
}
