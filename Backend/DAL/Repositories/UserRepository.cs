using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;


namespace DAL.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
}
