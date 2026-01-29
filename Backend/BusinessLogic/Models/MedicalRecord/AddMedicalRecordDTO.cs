using System.ComponentModel.DataAnnotations;

namespace BLL.Models.MedicalRecord;
public class AddMedicalRecordDTO
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    // Послуга може бути не вказана, якщо це загальний огляд
    public int? ServiceId { get; set; }

    [StringLength(2000)]
    public string? Result { get; set; }

    [Required(ErrorMessage = "Діагноз обов'язковий")]
    [StringLength(500)]
    public string? Diagnosis { get; set; }

    [StringLength(2000)]
    public string? Treatment { get; set; }

    [StringLength(2000)]
    public string? Recommendations { get; set; }
}
