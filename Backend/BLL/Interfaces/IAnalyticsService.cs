using BLL.Models.Analytics;

namespace BLL.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardAnalyticsDto> GetDashboardAsync(DateTime? startDate, DateTime? endDate);
}
