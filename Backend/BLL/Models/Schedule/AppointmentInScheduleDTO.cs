namespace BLL.Models.Schedule;
public class AppointmentInScheduleDTO
{
    public int AppointmentId { get; set; }
    public string StartTime { get; set; } = null!; // "10:30"
    public string EndTime { get; set; } = null!;   // "11:00"
    public string PatientFullName { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string Status { get; set; } = null!;
}

public class FreeSlotDTO
{
    public string Start { get; set; } = null!;
    public string End { get; set; } = null!;
}
