using System;

namespace Core.Events
{
    public record UserCreatedIntegrationEvent
    {
        public Guid UserId { get; init; }
        public string Username { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PasswordHash { get; init; } = string.Empty;
    }
}
