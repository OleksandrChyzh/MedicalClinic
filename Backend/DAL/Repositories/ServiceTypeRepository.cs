using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;


namespace DAL.Repositories;

public class ServiceTypeRepository(AppDbContext context) : BaseRepository<ServiceType>(context), IServiceTypeRepository
{
}
