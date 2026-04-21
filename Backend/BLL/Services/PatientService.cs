using AutoMapper;
using BLL.Interfaces;
using BLL.Models.Patient;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;
public class PatientService(IUnitOfWork unitOfWork, IMapper mapper) : IPatientService
{
    public async Task<IEnumerable<GetPatientDTO>> GetAllPatientsAsync()
    {
        var patients = await unitOfWork.PatientRepository.GetAllAsync(
            includes: [p => p.User, p => p.Appointments, p => p.MedicalRecords]
        );
        return mapper.Map<IEnumerable<GetPatientDTO>>(patients);
    }

    public async Task<IEnumerable<GetPatientDTO>> GetMyPatientsAsync(int currentUserId)
    {
        var patients = await unitOfWork.PatientRepository.GetAllAsync(
            filter: p => p.UserId == currentUserId,
            includes: [p => p.User, p => p.Appointments, p => p.MedicalRecords]
        );
        return mapper.Map<IEnumerable<GetPatientDTO>>(patients);
    }

    public async Task<GetPatientDTO> GetPatientByIdAsync(int patientId, int currentUserId, string role)
    {
        var patient = await unitOfWork.PatientRepository.GetFirstOrDefaultAsync(
            filter: p => p.Id == patientId,
            includes: [p => p.User, p => p.Appointments, p => p.MedicalRecords]
        );

        if (patient == null)
        {
            throw new KeyNotFoundException("Пацієнта не знайдено.");
        }

        // Якщо це звичайний юзер, він може бачити ТІЛЬКИ своїх пацієнтів.
        // Лікарі та Адміни можуть бачити всіх.
        if (role == "User" && patient.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Ви не маєте доступу до цього профілю пацієнта.");
        }

        return mapper.Map<GetPatientDTO>(patient);
    }

    public async Task<GetPatientDTO> CreatePatientAsync(AddPatientDTO dto, int currentUserId, string role)
    {
        // Захист від підміни UserId у JSON: примусово ставимо ID того, хто робить запит 
        // (якщо це тільки не адмін, який може створювати пацієнта для когось іншого)
        if (role != "Admin")
        {
            dto.UserId = currentUserId;
        }

        var patient = mapper.Map<Patient>(dto);
        await unitOfWork.PatientRepository.AddAsync(patient);

        // Підтягуємо створеного пацієнта з усіма зв'язками для коректного DTO
        return await this.GetPatientByIdAsync(patient.Id, currentUserId, role);
    }

    public async Task DeletePatientAsync(int patientId, int currentUserId, string role)
    {
        var patient = await unitOfWork.PatientRepository.GetByIdAsync(patientId);

        if (patient == null)
        {
            throw new KeyNotFoundException("Пацієнта не знайдено.");
        }

        // Дозвіл на видалення: власник-юзер або адмін
        if (role == "User" && patient.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Ви можете видаляти лише свої профілі пацієнтів.");
        }

        await unitOfWork.PatientRepository.DeleteAsync(patient);
    }
}
