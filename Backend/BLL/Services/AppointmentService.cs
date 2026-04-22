using AutoMapper;
using BLL.Interfaces;
using BLL.Models.Appointment;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;
public class AppointmentManagementService(IUnitOfWork unitOfWork, IMapper mapper) : IAppointmentService
{
    public async Task<IEnumerable<GetAppointmentDTO>> GetAllAppointmentsAsync()
    {
        var appointments = await unitOfWork.AppointmentRepository.GetAllAsync(
            includes: [a => a.Patient, a => a.Doctor, a => a.Service]
        );
        return mapper.Map<IEnumerable<GetAppointmentDTO>>(appointments);
    }

    public async Task<IEnumerable<GetAppointmentDTO>> GetAppointmentsByUserIdAsync(int userId)
    {
        var appointments = await unitOfWork.AppointmentRepository.GetAllAsync(
            filter: a => a.UserId == userId,
            includes: [a => a.Patient, a => a.Doctor, a => a.Service]
        );
        return mapper.Map<IEnumerable<GetAppointmentDTO>>(appointments);
    }

    public async Task<IEnumerable<GetAppointmentDTO>> GetAppointmentsByDoctorIdAsync(int doctorId)
    {
        var appointments = await unitOfWork.AppointmentRepository.GetAllAsync(
            filter: a => a.DoctorId == doctorId,
            includes: [a => a.Patient, a => a.Doctor, a => a.Service]
        );
        return mapper.Map<IEnumerable<GetAppointmentDTO>>(appointments);
    }

    // 1. Метод для ЗВИЧАЙНОГО ЮЗЕРА
    public async Task<GetAppointmentDTO> CreateAppointmentAsync(int userId, AddAppointmentDTO dto)
    {
        // Перевірка власності: чи пацієнт належить юзеру
        var patient = await unitOfWork.PatientRepository.GetByIdAsync(dto.PatientId);
        if (patient == null || patient.UserId != userId)
        {
            throw new UnauthorizedAccessException("Ви не можете записувати цього пацієнта.");
        }

        // Перевірка існування послуги (лікаря перевіримо в ValidateSlotAvailability)
        var serviceExists = await unitOfWork.ServiceRepository.GetByIdAsync(dto.ServiceId) != null;
        if (!serviceExists)
        {
            throw new KeyNotFoundException("Послугу не знайдено.");
        }

        // ВАЖЛИВО: Викликаємо спільну валідацію розкладу
        await ValidateSlotAvailability(dto);

        var appointment = mapper.Map<Appointment>(dto);
        appointment.UserId = userId; // Прив'язуємо до того, хто створив запис
        appointment.Status = AppointmentStatus.CREATED;

        await unitOfWork.AppointmentRepository.AddAsync(appointment);

        return await GetAppointmentByIdInternal(appointment.Id);
    }

    public async Task ChangeStatusAsync(int appointmentId, AppointmentStatus status)
    {
        var appointment = await unitOfWork.AppointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            throw new KeyNotFoundException("Запис не знайдено.");
        }

        appointment.Status = status;
        await unitOfWork.AppointmentRepository.UpdateAsync(appointment);
    }

    // Приватний метод для отримання повного DTO після створення
    private async Task<GetAppointmentDTO> GetAppointmentByIdInternal(int id)
    {
        var entity = await unitOfWork.AppointmentRepository.GetFirstOrDefaultAsync(
            filter: a => a.Id == id,
            includes: [a => a.Patient, a => a.Doctor, a => a.Service]
        );
        return mapper.Map<GetAppointmentDTO>(entity);
    }

    public async Task<GetAppointmentDTO> CreateAppointmentByDoctorAsync(int doctorUserId, AddAppointmentDTO dto)
    {
        // 1. Знаходимо самого лікаря по його UserId
        var doctor = await unitOfWork.DoctorRepository.GetFirstOrDefaultAsync(d => d.UserId == doctorUserId);
        if (doctor == null)
        {
            throw new UnauthorizedAccessException("Ви не зареєстровані як лікар.");
        }

        // Примусово ставимо DoctorId лікаря, який робить запит
        dto.DoctorId = doctor.Id;

        // 2. Валідація часу та розкладу
        await ValidateSlotAvailability(dto);

        // 3. Мапінг та збереження
        var appointment = mapper.Map<Appointment>(dto);
        // UserId у записі — це ID власника акаунта пацієнта (беремо з DTO)
        appointment.UserId = dto.UserId;
        appointment.Status = AppointmentStatus.CONFIRMED; // Лікар створює — значить уже підтверджено

        await unitOfWork.AppointmentRepository.AddAsync(appointment);

        return await GetAppointmentByIdInternal(appointment.Id);
    }

    // Приватний метод для перевірки накладок (Overlapping)
    private async Task ValidateSlotAvailability(AddAppointmentDTO dto)
    {
        var start = dto.AppointmentDate;
        var end = start.AddMinutes(dto.DurationMinutes);
        var day = start.DayOfWeek.ToString();

        // Перевірка робочого графіку (Schedule)
        var schedule = await unitOfWork.ScheduleRepository.GetFirstOrDefaultAsync(
            s => s.DoctorId == dto.DoctorId && s.WeekDay == day);

        if (schedule == null)
        {
            throw new Exception("Лікар не працює в цей день.");
        }

        var workStart = TimeOnly.FromDateTime(start);
        var workEnd = TimeOnly.FromDateTime(end);

        if (workStart < schedule.StartTime || workEnd > schedule.EndTime)
        {
            throw new Exception("Час виходить за межі робочої зміни лікаря.");
        }

        // ПЕРЕВІРКА НАКЛАДКИ: (StartA < EndB) AND (EndA > StartB)
        var overlap = await unitOfWork.AppointmentRepository.GetFirstOrDefaultAsync(a =>
            a.DoctorId == dto.DoctorId &&
            a.Status != AppointmentStatus.CANCELLED &&
            start < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
            end > a.AppointmentDate);

        if (overlap != null)
        {
            throw new Exception($"Цей час уже зайнятий: {overlap.AppointmentDate:HH:mm} - {overlap.AppointmentDate.AddMinutes(overlap.DurationMinutes):HH:mm}");
        }
    }
}
