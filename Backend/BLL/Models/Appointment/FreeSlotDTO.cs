namespace BLL.Models.Appointment;
public class FreeSlotDTO
{
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
}

public class AvailableSlotsResponseDTO
{
    public int DoctorId { get; set; }
    public DateTime Date { get; set; }
    public List<FreeSlotDTO> FreeSlots { get; set; } = [];
}
