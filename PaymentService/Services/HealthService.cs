using PaymentService.Infrastructure;

namespace PaymentService.Services;

public interface IHealthService
{
    Task<HealthCheckResult> CheckHealthAsync();
}

public sealed class HealthService : IHealthService
{
    private readonly PaymentDbContext _dbContext;

    public HealthService(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync()
    {
        var databaseAvailable = await _dbContext.Database.CanConnectAsync();
        return new HealthCheckResult(
            status: "PaymentService healthy",
            database: databaseAvailable ? "connected" : "unavailable");
    }
}

public sealed class HealthCheckResult
{
    public HealthCheckResult(string status, string database)
    {
        Status = status;
        Database = database;
    }

    public string Status { get; }
    public string Database { get; }
}
