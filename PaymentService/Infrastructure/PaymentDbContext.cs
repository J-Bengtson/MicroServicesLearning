using Microsoft.EntityFrameworkCore;
using Core.Domain;

namespace PaymentService.Infrastructure
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }

        // Add DbSets here
    }
}
