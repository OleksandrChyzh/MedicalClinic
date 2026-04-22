using BLL.Models.Review;

namespace BLL.Interfaces;
public interface IReviewService
{
    // Публічний доступ
    Task<IEnumerable<GetReviewDTO>> GetReviewsByDoctorAsync(int doctorId);
    Task<IEnumerable<GetReviewDTO>> GetReviewsByServiceAsync(int serviceId);

    // Доступ для користувача
    Task<GetReviewDTO> AddReviewAsync(AddReviewDTO dto, int userId);
    Task UpdateReviewAsync(UpdateReviewDTO dto, int userId);

    // Доступ для користувача та адміна
    Task DeleteReviewAsync(int reviewId, int userId, string role);
}
