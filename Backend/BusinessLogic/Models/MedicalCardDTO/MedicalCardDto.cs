using BLL.Models.MedicalRecord;

namespace BLL.Models.MedicalCardDTO;
public class MedicalCardDto
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    // Список усіх записів пацієнта
    public List<GetMedicalRecordDTO> Records { get; set; } = new();
}
