namespace DAL.Entities;

public class Appointment : IBaseEntity
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int ServiceId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.PENDING;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}
public enum AppointmentStatus
{
    PENDING,
    CONFIRMED,
    CANCELLED,
    COMPLETED
}
