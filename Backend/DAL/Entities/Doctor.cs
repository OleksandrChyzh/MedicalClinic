namespace DAL.Entities;

public class Doctor : IBaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int DirectionId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public int ExperienceYears { get; set; }
    public string? Description { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual Direction Direction { get; set; } = null!;
    public virtual ICollection<Schedule> Schedules { get; set; } = [];

    public virtual ICollection<Appointment> Appointments { get; set; } = [];

    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = [];

    public virtual ICollection<Review> Reviews { get; set; } = [];
}
