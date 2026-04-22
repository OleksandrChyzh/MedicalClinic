namespace BLL.Models.Schedule;
public class WeekScheduleDTO
{
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;

    // Список робочих днів (Пн-Пт тощо)
    public List<DailyScheduleDTO> WorkingDays { get; set; } = [];
}
