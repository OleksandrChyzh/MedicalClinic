namespace BLL.Models.Schedule;
public class DoctorDailyScheduleDTO
{
    public DateTime Date { get; set; }
    public string DayOfWeek { get; set; } = null!;
    public string WorkStartTime { get; set; } = null!;
    public string WorkEndTime { get; set; } = null!;

    // Список усіх записів на цей день
    public List<AppointmentInScheduleDTO> Appointments { get; set; } = [];
}

// 2. Вигляд для пацієнта (Тільки вільні вікна)
public class AvailableSlotsResponseDTO
{
    public int DoctorId { get; set; }
    public DateTime Date { get; set; }
    public List<FreeSlotDTO> FreeSlots { get; set; } = [];
}
