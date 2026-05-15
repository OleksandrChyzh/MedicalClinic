using DAL.Entities;

namespace DAL.Interfaces;

public interface IAppointmentRepository : IBaseRepository<Appointment>
{
    Task UpdateStatusAsync(Appointment appointment);
}
