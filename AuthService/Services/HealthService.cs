namespace AuthService.Services
{
    public interface IHealthService
    {
        Task<object> CheckHealthAsync();
    }

    public class HealthService : IHealthService
    {
        public async Task<object> CheckHealthAsync()
        {
            // Simple health check implementation
            return await Task.FromResult(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }
    }
}
