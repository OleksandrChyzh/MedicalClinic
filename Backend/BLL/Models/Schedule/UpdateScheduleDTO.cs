using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Schedule;
public class UpdateScheduleDTO
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "День тижня є обов'язковим")]
    [RegularExpression(@"^(Понеділок|Вівторок|Середа|Четвер|П'ятниця|Субота|Неділя)$",
    ErrorMessage = "Будь ласка, вкажіть коректний день тижня (наприклад, 'Понеділок')")]
    public string WeekDay { get; set; } = null!;

    [Required]
    [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "Формат часу має бути HH:mm")]
    public string StartTime { get; set; } = "08:00";

    [Required]
    [RegularExpression(@"^([01]\d|2[0-3]):([0-5]\d)$", ErrorMessage = "Формат часу має бути HH:mm")]
    public string EndTime { get; set; } = "17:00";
}
