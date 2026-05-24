namespace BLL.Models.Analytics;

public class DashboardAnalyticsDto
{
    public SummaryDto Summary { get; set; } = new();
    public List<ChartItemDto> RevenueByDirection { get; set; } = [];
    public List<ChartItemDto> AppointmentStatuses { get; set; } = [];
    public List<ChartItemDto> TopDirections { get; set; } = [];
    public List<DoctorKpiDto> DoctorKpi { get; set; } = [];
    public List<ChartItemDto> TrafficByMonth { get; set; } = [];
}

public class SummaryDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int CancelledAppointments { get; set; }
    public decimal NoShowRate { get; set; }
}

public class ChartItemDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class DoctorKpiDto
{
    public string DoctorName { get; set; } = string.Empty;
    public int CompletedCount { get; set; }
    public int TotalCount { get; set; }
}
