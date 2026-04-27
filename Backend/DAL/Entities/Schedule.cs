namespace DAL.Entities;
public partial class Schedule : IBaseEntity
{
    public int Id { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string WeekDay { get; set; } = null!;

    public int DoctorId { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;
}
