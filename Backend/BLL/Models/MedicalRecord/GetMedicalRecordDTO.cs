namespace BLL.Models.MedicalRecord;
public class GetMedicalRecordDTO
{
    public int Id { get; set; }

    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = string.Empty;

    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = string.Empty;

    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }

    public string? Result { get; set; }
    public string? Diagnosis { get; set; }
    public string? Treatment { get; set; }
    public string? Recommendations { get; set; }

    public DateTime CreatedAt { get; set; }
}
