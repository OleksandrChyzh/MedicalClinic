using BLL.Models.Schedule;

namespace BLL.Interfaces;
public interface IScheduleService
{
    // Читання (доступно всім)
    Task<IEnumerable<GetScheduleDTO>> GetDoctorScheduleAsync(int doctorId);
    Task<IEnumerable<GetScheduleDTO>> GetMyScheduleAsync(int userId);
    Task<GetScheduleDTO> GetScheduleByIdAsync(int id);

    // Модифікація (тільки для Admin)
    Task<GetScheduleDTO> CreateScheduleAsync(AddScheduleDTO dto);
    Task UpdateScheduleAsync(UpdateScheduleDTO dto);
    Task DeleteScheduleAsync(int id);
}
