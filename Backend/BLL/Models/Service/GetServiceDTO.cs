namespace BLL.Models.Service;
public class GetServiceDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public double AverageRating { get; set; }
    public int ReviewsCount { get; set; }
}
