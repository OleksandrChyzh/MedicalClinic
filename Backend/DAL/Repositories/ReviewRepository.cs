using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;

namespace DAL.Repositories
{
    public class ReviewRepository(AppDbContext context)
        : BaseRepository<Review>(context), IReviewRepository
    {
    }
}
