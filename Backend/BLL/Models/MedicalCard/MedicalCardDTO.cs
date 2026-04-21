using BLL.Models.MedicalRecord;

namespace BLL.Models.MedicalCard;
public class MedicalCardDTO
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    // Список усіх записів пацієнта
    public List<GetMedicalRecordDTO> Records { get; set; } = new();
}
