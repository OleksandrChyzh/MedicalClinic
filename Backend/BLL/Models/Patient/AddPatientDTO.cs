using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Patient;
public class AddPatientDTO
{
    [Required]
    public int UserId { get; set; } // Прив'язка до аккаунту (Auth)

    [Required(ErrorMessage = "Прізвище обов'язкове")]
    [StringLength(50, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ім'я обов'язкове")]
    [StringLength(50, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    // Додаємо валідацію, щоб не можна було поставити дату з майбутнього
    public DateTime BirthDate { get; set; }

    [Required]
    [RegularExpression("^(Male|Female)$", ErrorMessage = "Оберіть стать зі списку")]
    public string Gender { get; set; } = string.Empty;
}
