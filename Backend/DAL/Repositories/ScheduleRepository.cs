using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;

namespace DAL.Repositories
{
    public class ScheduleRepository(AppDbContext context) : BaseRepository<Schedule>(context), IScheduleRepository
    {
    }
}
