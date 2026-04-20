using System.ComponentModel.DataAnnotations;

namespace BLL.Models.Appointment;
public class AddAppointmentDTO
{
    [Required]
    public int PatientId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int DoctorId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime AppointmentDate { get; set; }
}
