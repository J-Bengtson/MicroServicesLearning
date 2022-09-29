using Core.Domain;

namespace AuthService.Domain
{
    public class LogAuthentication : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime LoginAttemptAt { get; set; } = DateTime.UtcNow;
        public bool IsSuccessful { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
