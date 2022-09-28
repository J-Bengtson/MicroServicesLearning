using Microsoft.EntityFrameworkCore;
using PaymentService.Services;

namespace PaymentService.Infrastructure;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly PaymentDbContext _dbContext;

    public DatabaseInitializer(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.MigrateAsync();
    }
}
