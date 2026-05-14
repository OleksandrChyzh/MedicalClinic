using BLL.Models.User;

namespace BLL.Models.Doctor;

/// <summary>
/// Повний профіль лікаря: публічні дані лікаря та обліковий запис користувача (GetUser).
/// </summary>
public class DoctorProfileDto
{
    public GetDoctorDto Doctor { get; set; } = null!;

    public GetUser User { get; set; } = null!;
}
