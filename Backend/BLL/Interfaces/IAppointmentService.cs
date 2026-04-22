using BLL.Models.Appointment;
using DAL.Entities;

namespace BLL.Interfaces;
public interface IAppointmentService
{
    // Адмін: всі записи
    Task<IEnumerable<GetAppointmentDTO>> GetAllAppointmentsAsync();

    // Користувач: тільки свої записи (де він є UserId)
    Task<IEnumerable<GetAppointmentDTO>> GetAppointmentsByUserIdAsync(int userId);

    // Лікар: записи до нього (за його DoctorId)
    Task<IEnumerable<GetAppointmentDTO>> GetAppointmentsByDoctorIdAsync(int doctorId);

    // Створення запису
    Task<GetAppointmentDTO> CreateAppointmentAsync(int userId, AddAppointmentDTO dto);

    // Зміна статусу (Лікар/Адмін)
    Task ChangeStatusAsync(int appointmentId, AppointmentStatus status);

    Task<GetAppointmentDTO> CreateAppointmentByDoctorAsync(int doctorUserId, AddAppointmentDTO dto);
}
