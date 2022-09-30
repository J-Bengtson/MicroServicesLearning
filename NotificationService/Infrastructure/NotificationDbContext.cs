using Microsoft.EntityFrameworkCore;
using Core.Domain;

namespace NotificationService.Infrastructure
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
        {
        }

        // Add DbSets here
    }
}
