using BLL.Models.MedicalCard;
using BLL.Models.MedicalRecord;

namespace BLL.Interfaces;
public interface IMedicalRecordService
{
    // Отримати всі записи (для Адміна)
    Task<IEnumerable<GetMedicalRecordDTO>> GetAllRecordsAsync();

    // Отримати конкретний запис
    Task<GetMedicalRecordDTO> GetRecordByIdAsync(int id, int currentUserId, string role);

    // Отримати повну медичну картку пацієнта (перевірка прав доступу всередині)
    Task<MedicalCardDTO> GetMedicalCardAsync(int patientId, int currentUserId, string role);

    // Створити запис (Лікар/Адмін)
    Task<GetMedicalRecordDTO> CreateMedicalRecordAsync(int currentUserId, string role, AddMedicalRecordDTO dto);
}
