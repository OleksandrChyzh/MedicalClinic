using AutoMapper;
using BLL.Interfaces;
using BLL.Models.Review;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;
public class ReviewService(IUnitOfWork unitOfWork, IMapper mapper) : IReviewService
{
    public async Task<IEnumerable<GetReviewDTO>> GetReviewsByDoctorAsync(int doctorId)
    {
        var reviews = await unitOfWork.ReviewRepository.GetAllAsync(
            filter: r => r.DoctorId == doctorId,
            includes: [r => r.User, r => r.Doctor!, r => r.Service!]
        );
        return mapper.Map<IEnumerable<GetReviewDTO>>(reviews);
    }

    public async Task<IEnumerable<GetReviewDTO>> GetReviewsByServiceAsync(int serviceId)
    {
        var reviews = await unitOfWork.ReviewRepository.GetAllAsync(
            filter: r => r.ServiceId == serviceId,
            includes: [r => r.User, r => r.Doctor!, r => r.Service!]
        );
        return mapper.Map<IEnumerable<GetReviewDTO>>(reviews);
    }

    public async Task<GetReviewDTO> AddReviewAsync(AddReviewDTO dto, int userId)
    {
        // Перевірка: відгук має бути або лікарю, або сервісу
        if (dto.DoctorId == null && dto.ServiceId == null)
        {
            throw new ArgumentException("Відгук повинен бути прив'язаний до лікаря або до послуги.");
        }

        var review = mapper.Map<Review>(dto);
        review.UserId = userId; // Примусово ставимо ID авторизованого юзера
        review.CreatedAt = DateTime.UtcNow;

        await unitOfWork.ReviewRepository.AddAsync(review);

        // Повертаємо DTO з підтягнутими іменами через внутрішній пошук
        var created = await unitOfWork.ReviewRepository.GetFirstOrDefaultAsync(
            filter: r => r.Id == review.Id,
            includes: [r => r.User, r => r.Doctor!, r => r.Service!]
        );
        return mapper.Map<GetReviewDTO>(created);
    }

    public async Task UpdateReviewAsync(UpdateReviewDTO dto, int userId)
    {
        var review = await unitOfWork.ReviewRepository.GetByIdAsync(dto.Id);

        if (review == null)
        {
            throw new KeyNotFoundException("Відгук не знайдено.");
        }

        // Тільки автор може редагувати
        if (review.UserId != userId)
        {
            throw new UnauthorizedAccessException("Ви можете редагувати тільки власні відгуки.");
        }

        mapper.Map(dto, review);
        await unitOfWork.ReviewRepository.UpdateAsync(review);
    }

    public async Task DeleteReviewAsync(int reviewId, int userId, string role)
    {
        var review = await unitOfWork.ReviewRepository.GetByIdAsync(reviewId);

        if (review == null)
        {
            throw new KeyNotFoundException("Відгук не знайдено.");
        }

        // Видалити може або автор, або Адмін
        if (role != "Admin" && review.UserId != userId)
        {
            throw new UnauthorizedAccessException("У вас немає прав на видалення цього відгуку.");
        }

        await unitOfWork.ReviewRepository.DeleteAsync(review);
    }
}
