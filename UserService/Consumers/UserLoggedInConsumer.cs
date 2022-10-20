using System.Threading.Tasks;
using MassTransit;
using Core.Events;
using Microsoft.Extensions.Logging;
using UserService.Infrastructure;

namespace UserService.Consumers
{
    public class UserLoggedInConsumer : IConsumer<UserLoggedInIntegrationEvent>
    {
        private readonly UserDbContext _dbContext;
        private readonly ILogger<UserLoggedInConsumer> _logger;

        public UserLoggedInConsumer(UserDbContext dbContext, ILogger<UserLoggedInConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UserLoggedInIntegrationEvent> context)
        {
            var loginEvent = context.Message;
            
            var user = await _dbContext.Users.FindAsync(loginEvent.UserId);
            if (user != null)
            {
                user.LastLoginDate = loginEvent.Timestamp;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("User {UserId} last login date updated to {Timestamp}", user.Id, loginEvent.Timestamp);
            }
            else
            {
                _logger.LogWarning("User {UserId} not found for login date update.", loginEvent.UserId);
            }
        }
    }
}
