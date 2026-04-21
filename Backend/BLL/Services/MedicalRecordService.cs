using AutoMapper;
using BLL.Interfaces;
using BLL.Models.MedicalCard;
using BLL.Models.MedicalRecord;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;
public class MedicalRecordService(IUnitOfWork unitOfWork, IMapper mapper) : IMedicalRecordService
{
    public async Task<IEnumerable<GetMedicalRecordDTO>> GetAllRecordsAsync()
    {
        var records = await unitOfWork.MedicalRecordRepository.GetAllAsync(
            includes: [r => r.Patient, r => r.Doctor, r => r.Service!]
        );
        return mapper.Map<IEnumerable<GetMedicalRecordDTO>>(records);
    }

    public async Task<GetMedicalRecordDTO> GetRecordByIdAsync(int id, int currentUserId, string role)
    {
        var record = await unitOfWork.MedicalRecordRepository.GetFirstOrDefaultAsync(
            filter: r => r.Id == id,
            includes: [r => r.Patient, r => r.Doctor, r => r.Service!]
        );

        if (record == null)
        {
            throw new KeyNotFoundException("Запис не знайдено.");
        }

        // Перевірка доступу: якщо це звичайний юзер, перевіряємо чи це його пацієнт
        if (role == "User" && record.Patient.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Ви не маєте доступу до цього запису.");
        }

        return mapper.Map<GetMedicalRecordDTO>(record);
    }

    public async Task<MedicalCardDTO> GetMedicalCardAsync(int patientId, int currentUserId, string role)
    {
        var patient = await unitOfWork.PatientRepository.GetByIdAsync(patientId);
        if (patient == null)
        {
            throw new KeyNotFoundException("Пацієнта не знайдено.");
        }

        // Перевірка прав власності на пацієнта
        if (role == "User" && patient.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("Ви не маєте доступу до медичної картки цього пацієнта.");
        }

        // Оскільки BaseRepository не підтримує ThenInclude, ми вантажимо записи окремо, 
        // де відразу підтягуємо Лікаря і Послугу
        var records = await unitOfWork.MedicalRecordRepository.GetAllAsync(
            filter: r => r.PatientId == patientId,
            includes: [r => r.Doctor, r => r.Service!]
        );

        // Мапимо пацієнта в шапку карти
        var card = mapper.Map<MedicalCardDTO>(patient);

        // Мапимо записи і вручну присвоюємо (перезаписуємо порожній список)
        card.Records = mapper.Map<List<GetMedicalRecordDTO>>(records);

        // Сортуємо записи від найновіших до найстаріших
        card.Records = card.Records.OrderByDescending(r => r.CreatedAt).ToList();

        return card;
    }

    public async Task<GetMedicalRecordDTO> CreateMedicalRecordAsync(int currentUserId, string role, AddMedicalRecordDTO dto)
    {
        // Якщо це лікар, він має право створювати записи тільки від свого імені
        if (role == "Doctor")
        {
            var doctor = await unitOfWork.DoctorRepository.GetFirstOrDefaultAsync(d => d.UserId == currentUserId);
            if (doctor == null || doctor.Id != dto.DoctorId)
            {
                throw new UnauthorizedAccessException("Лікар може створювати записи тільки від свого імені.");
            }
        }

        // Перевіряємо чи існує пацієнт
        var patientExists = await unitOfWork.PatientRepository.GetByIdAsync(dto.PatientId) != null;
        if (!patientExists)
        {
            throw new KeyNotFoundException("Пацієнта не знайдено.");
        }

        var record = mapper.Map<MedicalRecord>(dto);
        await unitOfWork.MedicalRecordRepository.AddAsync(record);

        // Повертаємо повноцінне DTO з підтягнутими іменами
        return await GetRecordByIdInternal(record.Id);
    }

    // Допоміжний приватний метод
    private async Task<GetMedicalRecordDTO> GetRecordByIdInternal(int id)
    {
        var record = await unitOfWork.MedicalRecordRepository.GetFirstOrDefaultAsync(
            filter: r => r.Id == id,
            includes: [r => r.Patient, r => r.Doctor, r => r.Service!]
        );
        return mapper.Map<GetMedicalRecordDTO>(record);
    }
}
