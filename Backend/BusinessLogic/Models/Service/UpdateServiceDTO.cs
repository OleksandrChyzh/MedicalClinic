using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Service;
public class UpdateServiceDTO
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Range(0.01, 100000)]
    public decimal Price { get; set; }
}
