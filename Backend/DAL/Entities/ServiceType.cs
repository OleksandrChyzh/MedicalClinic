namespace DAL.Entities;

public class ServiceType : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public virtual ICollection<Service>? Services { get; set; }
}
