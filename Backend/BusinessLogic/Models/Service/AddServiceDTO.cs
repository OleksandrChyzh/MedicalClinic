using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Service;
public class AddServiceDTO
{
    [Required(ErrorMessage = "Назва послуги обов'язкова")]
    [StringLength(150, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [Range(0.01, 100000, ErrorMessage = "Ціна має бути більшою за 0")]
    public decimal Price { get; set; }

    [Required]
    public int DirectionId { get; set; }

    [Required]
    public int TypeId { get; set; }
}
