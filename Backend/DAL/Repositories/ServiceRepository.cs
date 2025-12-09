using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;

namespace DAL.Repositories
{
    public class ServiceRepository(AppDbContext context) : BaseRepository<Service>(context), IServiceRepository
    {
    }
}
