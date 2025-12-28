namespace DAL.Entities;

public class MedicalRecord : IBaseEntity
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int? ServiceId { get; set; }
    public string? Result { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Recommendations { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
}
