using Core.Domain;

namespace UserService.Domain
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsPremium { get; set; } = false;
    }
}
