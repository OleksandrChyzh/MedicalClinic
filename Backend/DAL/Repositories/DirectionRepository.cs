using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;

namespace DAL.Repositories;

public class DirectionRepository : BaseRepository<Direction>, IDirectionRepository
{
    public DirectionRepository(AppDbContext context) : base(context) { }
}
