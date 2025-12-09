using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Repositories;

namespace DAL;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public IAppointmentRepository AppointmentRepository { get; set; } = new AppointmentRepository(context);
    public IDirectionRepository DirectionRepository { get; set; } = new DirectionRepository(context);
    public IDoctorRepository DoctorRepository { get; set; } = new DoctorRepository(context);
    public IMedicalRecordRepository MedicalRecordRepository { get; set; } = new MedicalRecordRepository(context);
    public IPatientRepository PatientRepository { get; set; } = new PatientRepository(context);
    public IReviewRepository ReviewRepository { get; set; } = new ReviewRepository(context);
    public IServiceRepository ServiceRepository { get; set; } = new ServiceRepository(context);
    public IScheduleRepository ScheduleRepository { get; set; } = new ScheduleRepository(context);

    public IUserRepository UserRepository { get; set; } = new UserRepository(context);

    public IServiceTypeRepository ServiceTypeRepository { get; set; } = new ServiceTypeRepository(context);
    public IDoctorRepository OrderRepository { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IMedicalRecordRepository OrderServiceRepository { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Task SaveAsync()
    {
        return context.SaveChangesAsync();
    }

    public IBaseRepository<TEntity> GetRepository<TEntity>()
        where TEntity : class, IBaseEntity
    {
        return typeof(TEntity).Name switch
        {
            nameof(Appointment) => (IBaseRepository<TEntity>)AppointmentRepository,
            nameof(Direction) => (IBaseRepository<TEntity>)DirectionRepository,
            nameof(Doctor) => (IBaseRepository<TEntity>)DoctorRepository,
            nameof(MedicalRecord) => (IBaseRepository<TEntity>)MedicalRecordRepository,
            nameof(Patient) => (IBaseRepository<TEntity>)PatientRepository,
            nameof(Schedule) => (IBaseRepository<TEntity>)ScheduleRepository,
            nameof(Service) => (IBaseRepository<TEntity>)ServiceRepository,
            nameof(ServiceType) => (IBaseRepository<TEntity>)ServiceTypeRepository,
            nameof(Review) => (IBaseRepository<TEntity>)ReviewRepository,
            nameof(User) => (IBaseRepository<TEntity>)UserRepository,
            _ => throw new InvalidOperationException($"Repository for type {typeof(TEntity)} not found"),
        };
    }
}
