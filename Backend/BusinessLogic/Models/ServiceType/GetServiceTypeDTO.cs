namespace BLL.Models.ServiceType;
public class GetServiceTypeDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Кількість послуг у цій категорії
    public int ServicesCount { get; set; }
}
