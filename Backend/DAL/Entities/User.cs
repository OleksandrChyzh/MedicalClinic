using Microsoft.AspNetCore.Identity;
namespace DAL.Entities;

public partial class User : IdentityUser<int>, IBaseEntity
{
    public virtual ICollection<Patient> Patients { get; set; } = [];
    public virtual Doctor? Doctor { get; set; }
    public virtual ICollection<Review> Reviews { get; set; } = [];

}
