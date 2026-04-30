namespace BLL.Models.Doctor;
public class GetDoctorDto
{
    public int Id { get; set; }

    // Склеєне ім'я для виводу в списку
    public string FullName { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }
    public string? Description { get; set; }

    // Дані про напрямок
    public int DirectionId { get; set; }
    public string DirectionName { get; set; } = string.Empty;

    // Середній рейтинг (наприклад, 4.8)
    public double AverageRating { get; set; }

    // Загальна кількість (зручно для UI: "4.8 (120 відгуків)")
    public int ReviewsCount { get; set; }

    // Список ID відгуків для можливої деталізації або фільтрації
    public List<int> ReviewIds { get; set; } = new();

    // Email або Phone з сутності User
    public string ContactPhone { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

}
