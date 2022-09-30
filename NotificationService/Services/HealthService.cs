using System;
using System.Threading.Tasks;

namespace NotificationService.Services
{
    public interface IHealthService
    {
        Task<object> CheckHealthAsync();
    }

    public class HealthService : IHealthService
    {
        public async Task<object> CheckHealthAsync()
        {
            return await Task.FromResult(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }
    }
}
