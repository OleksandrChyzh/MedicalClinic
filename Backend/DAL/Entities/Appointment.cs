namespace DAL.Entities;

public class Appointment : IBaseEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int ServiceId { get; set; }
    public DateTime AppointmentDate { get; set; }

    public int DurationMinutes { get; set; } // Скільки триває запис
    public AppointmentStatus Status { get; set; } = AppointmentStatus.CREATED;
    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
    public virtual Patient Patient { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}
public enum AppointmentStatus
{
    CREATED,
    CONFIRMED,
    CANCELLED,
    COMPLETED
}
