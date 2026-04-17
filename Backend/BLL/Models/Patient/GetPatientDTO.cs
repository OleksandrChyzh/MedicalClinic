namespace BLL.Models.Patient;
public class GetPatientDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Для відображення "Прізвище Ім'я"
    public string FullName => $"{LastName} {FirstName}";

    public DateTime BirthDate { get; set; }

    // Зручно для швидкого перегляду в списку
    public int Age => DateTime.Today.Year - BirthDate.Year -
                     (BirthDate.Date > DateTime.Today.AddYears(-(DateTime.Today.Year - BirthDate.Year)) ? 1 : 0);

    public string Gender { get; set; } = string.Empty;

    // Дані з сутності User (через Join)
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    // Статистика
    public int AppointmentsCount { get; set; }
    public int RecordsCount { get; set; }
}
