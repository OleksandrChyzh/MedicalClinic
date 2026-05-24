using Microsoft.AspNetCore.Identity;
namespace DAL.Entities;

public partial class User : IdentityUser<int>, IBaseEntity
{
    public bool IsBlocked { get; set; }
    public virtual ICollection<Patient> Patients { get; set; } = [];
    public virtual Doctor? Doctor { get; set; }
    public virtual ICollection<Review> Reviews { get; set; } = [];
    public virtual ICollection<Appointment> Appointments { get; set; } = [];
}
