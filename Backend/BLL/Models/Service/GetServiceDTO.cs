namespace BLL.Models.Service;
public class GetServiceDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }

    /// <summary>Напрямок клініки (для попереднього заповнення запису на прийом).</summary>
    public int DirectionId { get; set; }

    /// <summary>Тип послуги (відповідає TypeId у сутності Service).</summary>
    public int TypeId { get; set; }
}
