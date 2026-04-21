using BLL.Models.Patient;

namespace BLL.Interfaces;
public interface IPatientService
{
    // Для Адміна
    Task<IEnumerable<GetPatientDTO>> GetAllPatientsAsync();

    // Для Юзера (перегляд своїх пацієнтів)
    Task<IEnumerable<GetPatientDTO>> GetMyPatientsAsync(int currentUserId);

    // Для всіх авторизованих (з внутрішньою перевіркою доступу)
    Task<GetPatientDTO> GetPatientByIdAsync(int patientId, int currentUserId, string role);

    // Створення пацієнта
    Task<GetPatientDTO> CreatePatientAsync(AddPatientDTO dto, int currentUserId, string role);

    // Видалення пацієнта
    Task DeletePatientAsync(int patientId, int currentUserId, string role);
}
