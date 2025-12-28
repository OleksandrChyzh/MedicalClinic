namespace DAL.Entities;

public class Review : IBaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? DoctorId { get; set; }
    public int? ServiceId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual User User { get; set; } = null!;
    public virtual Doctor? Doctor { get; set; }
    public virtual Service? Service { get; set; }
}
