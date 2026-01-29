namespace BLL.Models.Review;
public class GetReviewDTO
{
    public int Id { get; set; }

    // Хто залишив
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;

    // Кому/Чому залишив
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }

    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }

    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
