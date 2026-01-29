using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Review;
public class UpdateReviewDTO
{
    [Required]
    public int Id { get; set; }

    [Required]
    [Range(1, 5, ErrorMessage = "Рейтинг має бути від 1 до 5 зірок")]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }
}
