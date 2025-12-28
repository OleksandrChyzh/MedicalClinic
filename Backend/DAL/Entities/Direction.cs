namespace DAL.Entities;
public class Direction : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public virtual ICollection<Doctor> Doctors { get; set; } = [];

    public virtual ICollection<Service> Services { get; set; } = [];
}
