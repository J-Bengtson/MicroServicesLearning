using System;

namespace Core.Events
{
    public class UserLoggedInIntegrationEvent
    {
        public Guid UserId { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = string.Empty;
    }
}
