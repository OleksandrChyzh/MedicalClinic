using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Review;
public class AddReviewDTO
{
    [Required]
    public int UserId { get; set; }

    // Хоча б один з цих ID має бути заповнений
    public int? DoctorId { get; set; }
    public int? ServiceId { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Рейтинг має бути від 1 до 5 зірок")]
    public int Rating { get; set; }

    [StringLength(1000, ErrorMessage = "Коментар не може бути довшим за 1000 символів")]
    public string? Comment { get; set; }
}
