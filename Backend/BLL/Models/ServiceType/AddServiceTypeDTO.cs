using System.ComponentModel.DataAnnotations;

namespace BLL.Models.ServiceType;
public class AddServiceTypeDTO
{
    [Required(ErrorMessage = "Назва типу послуги є обов'язковою")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Назва має бути від 3 до 100 символів")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
    public string? Description { get; set; }
}
