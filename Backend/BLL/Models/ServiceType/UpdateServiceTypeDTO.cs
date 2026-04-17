using System.ComponentModel.DataAnnotations;

namespace BLL.Models.ServiceType;
public class UpdateServiceTypeDTO
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Назва обов'язкова")]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}
