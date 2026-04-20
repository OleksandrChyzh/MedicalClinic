namespace BLL.Models.Appointment;
public class GetAppointmentDTO
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientFullName { get; set; } = null!;
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; } = null!;
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
