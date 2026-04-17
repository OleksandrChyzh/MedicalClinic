using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Schedule;
public class AddScheduleDTO
{
    [Required]
    public int DoctorId { get; set; }

    [Required(ErrorMessage = "День тижня є обов'язковим")]
    [RegularExpression(@"^(Понеділок|Вівторок|Середа|Четвер|П'ятниця|Субота|Неділя)$",
    ErrorMessage = "Будь ласка, вкажіть коректний день тижня (наприклад, 'Понеділок')")]
    public string WeekDay { get; set; } = null!;

    [Required]
    public string StartTime { get; set; } = "08:00"; 

    [Required]
    public string EndTime { get; set; } = "17:00";
}
