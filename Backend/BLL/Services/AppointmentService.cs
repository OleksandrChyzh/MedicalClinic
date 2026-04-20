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

    public async Task<GetAppointmentDTO> CreateAppointmentAsync(int userId, AddAppointmentDTO dto)
    {
        // ВАЖЛИВО: Перевіряємо, чи цей пацієнт належить поточному користувачу
        var patient = await unitOfWork.PatientRepository.GetByIdAsync(dto.PatientId);
        if (patient == null || patient.UserId != userId)
        {
            throw new UnauthorizedAccessException("Ви не можете записувати цього пацієнта.");
        }

        // Перевірка існування лікаря та послуги
        var doctorExists = await unitOfWork.DoctorRepository.GetByIdAsync(dto.DoctorId) != null;
        var serviceExists = await unitOfWork.ServiceRepository.GetByIdAsync(dto.ServiceId) != null;

        if (!doctorExists || !serviceExists)
        {
            throw new KeyNotFoundException("Лікаря або послугу не знайдено.");
        }

        var appointment = mapper.Map<Appointment>(dto);
        appointment.UserId = userId; // Прив'язуємо до власника акаунта

        await unitOfWork.AppointmentRepository.AddAsync(appointment);

        // Повертаємо з усіма включеними даними для відображення
        return await this.GetAppointmentByIdInternal(appointment.Id);
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
}
