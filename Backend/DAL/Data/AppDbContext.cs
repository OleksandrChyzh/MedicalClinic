using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data;

public class AppDbContext : IdentityDbContext<
    User,
    IdentityRole<int>,
    int,
    IdentityUserClaim<int>,
    IdentityUserRole<int>,
    IdentityUserLogin<int>,
    IdentityRoleClaim<int>,
    IdentityUserToken<int>>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Direction> Directions { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Schedule> Schedules { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // IGNORE UNUSED IDENTITY TABLES
        builder.Ignore<IdentityUserClaim<int>>();
        builder.Ignore<IdentityUserLogin<int>>();
        builder.Ignore<IdentityUserToken<int>>();
        builder.Ignore<IdentityRoleClaim<int>>();

        // =============================
        // USER TABLE
        // =============================
        builder.Entity<User>(b =>
        {
            b.ToTable("Users", t =>
            {
                t.HasCheckConstraint("CHK_User_Email_Format", "\"Email\" ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");
                t.HasCheckConstraint("CHK_User_Phone_Format", "\"PhoneNumber\" ~ '^\\+?[0-9]{7,20}$'");
            });

            b.Property(u => u.Email).IsRequired().HasMaxLength(50);
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(20);
            b.Property(u => u.UserName).IsRequired().HasMaxLength(50);

            b.Ignore(u => u.AccessFailedCount);
            b.Ignore(u => u.LockoutEnabled);
            b.Ignore(u => u.LockoutEnd);
            b.Ignore(u => u.TwoFactorEnabled);
            b.Ignore(u => u.EmailConfirmed);
            b.Ignore(u => u.PhoneNumberConfirmed);
            b.Ignore(u => u.SecurityStamp);
            b.Ignore(u => u.ConcurrencyStamp);
        });

        builder.Entity<User>()
            .HasMany(u => u.Patients)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<User>()
            .HasOne(u => u.Doctor)
            .WithOne(d => d.User)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<User>()
            .HasMany(u => u.Reviews)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // =============================
        // ROLES & USERROLES
        // =============================
        builder.Entity<IdentityRole<int>>(b => b.ToTable("Roles"));
        builder.Entity<IdentityUserRole<int>>(b =>
        {
            b.HasKey(ur => new { ur.UserId, ur.RoleId });
            b.ToTable("UserRoles");
        });

        // ============ PATIENT ============
        builder.Entity<Patient>(b =>
        {
            b.ToTable("Patients", t =>
            {
                t.HasCheckConstraint("CHK_Patient_BirthDate", "\"BirthDate\" <= CURRENT_DATE");
                t.HasCheckConstraint("CHK_Patient_Gender", "\"Gender\" IN ('Male', 'Female')");
            });

            b.HasKey(p => p.Id);
            b.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            b.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            b.Property(p => p.BirthDate).IsRequired();
            b.Property(p => p.Gender).IsRequired().HasMaxLength(10);
        });

        // ============ DOCTOR ============
        builder.Entity<Doctor>(b =>
        {
            b.ToTable("Doctors", t =>
                t.HasCheckConstraint("CHK_Doctor_ExperienceYears", "\"ExperienceYears\" >= 0"));

            b.HasKey(d => d.Id);
            b.Property(d => d.LastName).IsRequired().HasMaxLength(100);
            b.Property(d => d.FirstName).IsRequired().HasMaxLength(100);
            b.Property(d => d.MiddleName).HasMaxLength(100);
            b.Property(d => d.Description).HasMaxLength(1000);

            b.HasOne(d => d.Direction)
                .WithMany(dir => dir.Doctors)
                .HasForeignKey(d => d.DirectionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ DIRECTIONS ============
        builder.Entity<Direction>(b =>
        {
            b.ToTable("Directions");
            b.HasKey(d => d.Id);
            b.Property(d => d.Name).IsRequired().HasMaxLength(100);
            b.Property(d => d.Description).HasMaxLength(1000);
        });

        // ============ SERVICE TYPES ============
        builder.Entity<ServiceType>(b =>
        {
            b.ToTable("ServiceTypes");
            b.HasKey(t => t.Id);
            b.Property(t => t.Name).IsRequired().HasMaxLength(100);
            b.Property(t => t.Description).HasMaxLength(1000);
        });

        // ============ SERVICES ============
        builder.Entity<Service>(b =>
        {
            b.ToTable("Services", t =>
                t.HasCheckConstraint("CHK_Service_Price_NonNegative", "\"Price\" >= 0"));

            b.HasKey(s => s.Id);
            b.Property(s => s.Name).IsRequired().HasMaxLength(150);
            b.Property(s => s.Price).IsRequired().HasPrecision(10, 2);

            b.HasOne(s => s.ServiceType)
                .WithMany(st => st.Services)
                .HasForeignKey(s => s.TypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ APPOINTMENTS ============
        builder.Entity<Appointment>(b =>
        {
            b.ToTable("Appointments", t =>
                t.HasCheckConstraint("CHK_Appointment_Date_Future", "\"AppointmentDate\" >= NOW()"));

            b.HasKey(a => a.Id);
            b.Property(a => a.Status).IsRequired().HasConversion<string>();
            b.Property(a => a.CreatedAt).HasDefaultValueSql("NOW()").IsRequired();
        });

        // ============ REVIEWS ============
        builder.Entity<Review>(b =>
        {
            b.ToTable("Reviews", t =>
                t.HasCheckConstraint("CHK_Review_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5"));

            b.Property(r => r.Rating).IsRequired().HasDefaultValue(1);
            b.Property(r => r.Comment).HasMaxLength(1000);
            b.Property(r => r.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // ============ SCHEDULE ============
        builder.Entity<Schedule>(b =>
        {
            b.ToTable("Schedules", t =>
                t.HasCheckConstraint("CK_Schedules_WeekDay",
                "\"WeekDay\" IN ('Понеділок','Вівторок','Середа','Четвер','П''ятниця','Субота','Неділя')"));

            b.HasKey(e => e.Id);
            b.Property(e => e.StartTime).IsRequired().HasColumnType("time");
            b.Property(e => e.EndTime).IsRequired().HasColumnType("time");
            b.Property(e => e.WeekDay).IsRequired().HasMaxLength(20);
        });

        // ============ MEDICAL RECORD ============
        builder.Entity<MedicalRecord>(b =>
        {
            b.ToTable("MedicalRecords");
            b.HasKey(e => e.Id);
            b.Property(e => e.Diagnosis).HasColumnType("text");
            b.Property(e => e.Treatment).HasColumnType("text");
            b.Property(e => e.CreatedAt).HasColumnType("timestamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
