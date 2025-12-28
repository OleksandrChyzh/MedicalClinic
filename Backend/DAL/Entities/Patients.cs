namespace DAL.Entities;

public class Patient : IBaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public required string Gender { get; set; }
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = [];
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = [];
}
