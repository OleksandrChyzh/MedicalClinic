namespace BLL.Models.Direction;
public class GetDirectionDTO
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Кількість для відображення статистики (наприклад, "15 лікарів у цьому напрямку")
    public int DoctorsCount { get; set; }

    public int ServicesCount { get; set; }

    // Опціонально: список імен послуг, щоб показати їх відразу без додаткових запитів
    public List<string> ServiceNames { get; set; } = [];
}
