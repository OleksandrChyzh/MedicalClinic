namespace BLL.Models.Schedule;
public class GetScheduleDTO
{
    public int Id { get; set; }
    public string WeekDay { get; set; } = null!;
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
}
