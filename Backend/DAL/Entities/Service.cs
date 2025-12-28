namespace DAL.Entities;

public class Service : IBaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DirectionId { get; set; }
    public int TypeId { get; set; }
    public virtual Direction Direction { get; set; } = null!;
    public virtual ServiceType ServiceType { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = [];
    public virtual ICollection<Review> Reviews { get; set; } = [];
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = [];
}
