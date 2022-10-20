using System;
using System.Threading.Tasks;
using MassTransit;
using Core.Events;
using Microsoft.Extensions.Logging;

namespace NotificationService.Consumers
{
    public class SecurityAlertConsumer : IConsumer<UserLoggedInIntegrationEvent>
    {
        private readonly ILogger<SecurityAlertConsumer> _logger;

        public SecurityAlertConsumer(ILogger<SecurityAlertConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<UserLoggedInIntegrationEvent> context)
        {
            var loginEvent = context.Message;
            
            _logger.LogInformation("\n===================================\n" +
                                   "🚨 SECURITY ALERT (NEW LOGIN)\n" +
                                   "To: User {UserId}\n" +
                                   "Time: {Timestamp}\n" +
                                   "IP: {IpAddress}\n" +
                                   "Message: A new login was detected on your account. If this wasn't you, please change your password immediately.\n" +
                                   "===================================\n", 
                                   loginEvent.UserId, loginEvent.Timestamp, loginEvent.IpAddress);

            return Task.CompletedTask;
        }
    }
}
