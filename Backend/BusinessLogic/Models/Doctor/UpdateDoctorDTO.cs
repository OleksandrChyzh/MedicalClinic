using System.ComponentModel.DataAnnotations;
namespace BLL.Models.Doctor;
public class UpdateDoctorDTO
{
    [Required]
    public int Id { get; set; }

    [Required]
    public int DirectionId { get; set; }

    [Required(ErrorMessage = "Прізвище є обов'язковим")]
    [StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ім'я є обов'язковим")]
    [StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? MiddleName { get; set; }

    [Range(0, 70, ErrorMessage = "Досвід має бути від 0 до 70 років")]
    public int ExperienceYears { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }
}
