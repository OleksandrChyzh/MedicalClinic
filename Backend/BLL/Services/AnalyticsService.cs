using BLL.Interfaces;
using BLL.Models.Analytics;
using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class AnalyticsService(AppDbContext db) : IAnalyticsService
{
    public async Task<DashboardAnalyticsDto> GetDashboardAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = db.Appointments
            .Include(a => a.Service)
                .ThenInclude(s => s.Direction)
            .Include(a => a.Doctor)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(a => a.AppointmentDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(a => a.AppointmentDate <= endDate.Value);

        var appointments = await query.ToListAsync();

        var summary = BuildSummary(appointments);
        var revenueByDirection = BuildRevenueByDirection(appointments);
        var statuses = BuildStatusDistribution(appointments);
        var topDirections = BuildTopDirections(appointments);
        var doctorKpi = BuildDoctorKpi(appointments);
        var traffic = BuildTrafficByMonth(appointments);

        return new DashboardAnalyticsDto
        {
            Summary = summary,
            RevenueByDirection = revenueByDirection,
            AppointmentStatuses = statuses,
            TopDirections = topDirections,
            DoctorKpi = doctorKpi,
            TrafficByMonth = traffic
        };
    }

    private static SummaryDto BuildSummary(List<Appointment> appointments)
    {
        var total = appointments.Count;
        var completed = appointments.Count(a => a.Status == AppointmentStatus.COMPLETED);
        var cancelled = appointments.Count(a => a.Status == AppointmentStatus.CANCELLED);
        var revenue = appointments
            .Where(a => a.Status == AppointmentStatus.COMPLETED)
            .Sum(a => a.Service?.Price ?? 0);

        var noShowRate = total > 0 ? Math.Round((decimal)cancelled / total * 100, 1) : 0;

        return new SummaryDto
        {
            TotalRevenue = revenue,
            TotalAppointments = total,
            CompletedAppointments = completed,
            CancelledAppointments = cancelled,
            NoShowRate = noShowRate
        };
    }

    private static List<ChartItemDto> BuildRevenueByDirection(List<Appointment> appointments)
    {
        return appointments
            .Where(a => a.Status == AppointmentStatus.COMPLETED && a.Service?.Direction != null)
            .GroupBy(a => a.Service.Direction.Name)
            .Select(g => new ChartItemDto
            {
                Label = g.Key,
                Value = g.Sum(a => a.Service.Price)
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    private static List<ChartItemDto> BuildStatusDistribution(List<Appointment> appointments)
    {
        return appointments
            .GroupBy(a => a.Status.ToString())
            .Select(g => new ChartItemDto
            {
                Label = g.Key,
                Value = g.Count()
            })
            .ToList();
    }

    private static List<ChartItemDto> BuildTopDirections(List<Appointment> appointments)
    {
        return appointments
            .Where(a => a.Status == AppointmentStatus.COMPLETED && a.Service?.Direction != null)
            .GroupBy(a => a.Service.Direction.Name)
            .Select(g => new ChartItemDto
            {
                Label = g.Key,
                Value = g.Count()
            })
            .OrderByDescending(x => x.Value)
            .Take(10)
            .ToList();
    }

    private static List<DoctorKpiDto> BuildDoctorKpi(List<Appointment> appointments)
    {
        return appointments
            .GroupBy(a => new { a.DoctorId, DoctorName = $"{a.Doctor.LastName} {a.Doctor.FirstName}" })
            .Select(g => new DoctorKpiDto
            {
                DoctorName = g.Key.DoctorName,
                CompletedCount = g.Count(a => a.Status == AppointmentStatus.COMPLETED),
                TotalCount = g.Count()
            })
            .OrderByDescending(x => x.CompletedCount)
            .Take(10)
            .ToList();
    }

    private static List<ChartItemDto> BuildTrafficByMonth(List<Appointment> appointments)
    {
        return appointments
            .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
            .Select(g => new ChartItemDto
            {
                Label = $"{g.Key.Month:D2}.{g.Key.Year}",
                Value = g.Count()
            })
            .OrderBy(x => x.Label)
            .ToList();
    }
}
