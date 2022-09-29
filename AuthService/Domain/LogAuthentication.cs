using System;

namespace AuthService.Domain
{
    public class LogAuthentication
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public DateTime LoginAttemptAt { get; set; } = DateTime.UtcNow;
        public bool IsSuccessful { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
