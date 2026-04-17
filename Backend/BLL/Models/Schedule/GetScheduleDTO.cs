namespace BLL.Models.Schedule;
public class GetScheduleDTO
{
    public int Id { get; set; }

    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;

    public string WeekDay { get; set; } = null!;

    // Повертаємо час як рядок для легкого відображення в Angular
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
}
