using DAL.Entities;


namespace DAL.Interfaces;

public interface IUnitOfWork
{
    IAppointmentRepository AppointmentRepository { get; set; }
    IDirectionRepository DirectionRepository { get; set; }
    IDoctorRepository OrderRepository { get; set; }
    IMedicalRecordRepository OrderServiceRepository { get; set; }
    IPatientRepository PatientRepository { get; set; }
    IScheduleRepository ScheduleRepository { get; set; }
    IServiceRepository ServiceRepository { get; set; }
    IUserRepository UserRepository { get; set; }
    IServiceTypeRepository ServiceTypeRepository { get; set; }
    IReviewRepository ReviewRepository { get; set; }
    Task SaveAsync();
    IBaseRepository<TEntity> GetRepository<TEntity>() where TEntity : class, IBaseEntity;
}
