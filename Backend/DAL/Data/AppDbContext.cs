using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data
{
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
        public virtual DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // IGNORE UNUSED IDENTITY TABLES
            // =============================
            builder.Ignore<IdentityUserClaim<int>>();
            builder.Ignore<IdentityUserLogin<int>>();
            builder.Ignore<IdentityUserToken<int>>();
            builder.Ignore<IdentityRoleClaim<int>>();

            // =============================
            // USER TABLE
            // =============================
            builder.Entity<User>(b =>
            {
                b.ToTable("Users");

                b.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(50);

                b.HasIndex(u => u.Email).IsUnique();

                b.Property(u => u.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                b.Property(u => u.UserName)
                    .IsRequired()
                    .HasMaxLength(50);

                // Перевірка формату Email
                b.HasCheckConstraint("CHK_User_Email_Format",
                    "\"Email\" ~ '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$'");

                // Перевірка формату PhoneNumber (лише цифри та + на початку)
                b.HasCheckConstraint("CHK_User_Phone_Format",
                    "\"PhoneNumber\" ~ '^\\+?[0-9]{7,20}$'");

                // Ігноруємо непотрібні поля Identity
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
            // ROLES TABLE
            // =============================
            builder.Entity<IdentityRole<int>>(b =>
            {
                b.ToTable("Roles");
            });

            // =============================
            // USERROLES TABLE
            // =============================
            builder.Entity<IdentityUserRole<int>>(b =>
            {
                b.HasKey(ur => new { ur.UserId, ur.RoleId });
                b.ToTable("UserRoles");
            });


            // ============ PATIENT ============

            builder.Entity<Patient>(b =>
            {
                b.ToTable("Patients");

                // PRIMARY KEY
                b.HasKey(p => p.Id);

                // FOREIGN KEY на User
                b.HasOne(p => p.User)
                    .WithMany(u => u.Patients)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // LastName та FirstName обов'язкові, обмежимо довжину
                b.Property(p => p.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(p => p.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                // BirthDate обов'язковий, перевіримо, що дата не в майбутньому
                b.Property(p => p.BirthDate)
                    .IsRequired();

                b.HasCheckConstraint("CHK_Patient_BirthDate",
                    "\"BirthDate\" <= CURRENT_DATE");

                // Gender обов'язковий, обмежимо конкретними значеннями
                b.Property(p => p.Gender)
                    .IsRequired()
                    .HasMaxLength(10);

                b.HasCheckConstraint("CHK_Patient_Gender",
                    "\"Gender\" IN ('Male', 'Female')");

                // Відношення з Appointments
                b.HasMany(p => p.Appointments)
                    .WithOne(a => a.Patient)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Відношення з MedicalRecords
                b.HasMany(p => p.MedicalRecords)
                    .WithOne(m => m.Patient)
                    .HasForeignKey(m => m.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ DOCTOR ============

            builder.Entity<Doctor>(b =>
            {
                b.ToTable("Doctors");

                // PRIMARY KEY
                b.HasKey(d => d.Id);

                // FOREIGN KEY на User
                b.HasOne(d => d.User)
                    .WithOne(u => u.Doctor)
                    .HasForeignKey<Doctor>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // FOREIGN KEY на Direction
                b.HasOne(d => d.Direction)
                    .WithMany(dir => dir.Doctors)
                    .HasForeignKey(d => d.DirectionId)
                    .OnDelete(DeleteBehavior.Restrict);

                // LastName та FirstName обов'язкові, обмежимо довжину
                b.Property(d => d.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                b.Property(d => d.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                // MiddleName не обов'язкове, обмежимо довжину
                b.Property(d => d.MiddleName)
                    .HasMaxLength(100);

                // ExperienceYears обов'язковий, додамо перевірку на невід’ємне число
                b.Property(d => d.ExperienceYears)
                    .IsRequired();

                b.HasCheckConstraint("CHK_Doctor_ExperienceYears",
                    "\"ExperienceYears\" >= 0");

                // Description необов'язковий, обмежимо довжину
                b.Property(d => d.Description)
                    .HasMaxLength(1000);

                // ============ ВІДНОШЕННЯ ============
                b.HasMany(d => d.Appointments)
                    .WithOne(a => a.Doctor)
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(d => d.MedicalRecords)
                    .WithOne(m => m.Doctor)
                    .HasForeignKey(m => m.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(d => d.Reviews)
                    .WithOne(r => r.Doctor)
                    .HasForeignKey(r => r.DoctorId)
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasMany(d => d.Schedules)
                    .WithOne(s => s.Doctor)
                    .HasForeignKey(s => s.DoctorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // ============ DIRECTIONS ============
            builder.Entity<Direction>(b =>
            {
                b.ToTable("Directions");

                // PRIMARY KEY
                b.HasKey(d => d.Id);

                // Назва напряму обов'язкова, максимум 100 символів
                b.Property(d => d.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                // Опис необов'язковий, обмежимо довжину 1000 символів
                b.Property(d => d.Description)
                    .HasMaxLength(1000);

                // ============ ВІДНОШЕННЯ ============
                // Лікарі
                b.HasMany(d => d.Doctors)
                    .WithOne(doc => doc.Direction)
                    .HasForeignKey(doc => doc.DirectionId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Послуги
                b.HasMany(d => d.Services)
                    .WithOne(s => s.Direction)
                    .HasForeignKey(s => s.DirectionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // ============ SERVICE TYPES ============
            builder.Entity<ServiceType>(b =>
            {
                b.ToTable("ServiceTypes");

                // PRIMARY KEY
                b.HasKey(t => t.Id);

                // Назва типу послуги обов'язкова, максимум 100 символів
                b.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                // Опис необов'язковий, максимум 1000 символів
                b.Property(t => t.Description)
                    .HasMaxLength(1000);

                // ============ ВІДНОШЕННЯ ============
                b.HasMany(t => t.Services)
                    .WithOne(s => s.ServiceType)
                    .HasForeignKey(s => s.TypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Додатково можна зробити перевірку формату Name, наприклад, щоб не було цифр
                // b.HasCheckConstraint("CHK_ServiceType_Name_Format", "\"Name\" ~ '^[A-Za-zА-Яа-я\\s-]+$'");
            });


            // ============ SERVICES ============
            builder.Entity<Service>(b =>
            {
                b.ToTable("Services");

                // PRIMARY KEY
                b.HasKey(s => s.Id);

                // Назва послуги обов'язкова, максимум 150 символів
                b.Property(s => s.Name)
                    .IsRequired()
                    .HasMaxLength(150);

                // Опис необов'язковий, максимум 1000 символів
                b.Property(s => s.Description)
                    .HasMaxLength(1000);

                // Ціна обов'язкова, не менше 0
                b.Property(s => s.Price)
                    .IsRequired()
                    .HasPrecision(10, 2); // наприклад, до 99999999.99
                b.HasCheckConstraint("CHK_Service_Price_NonNegative", "\"Price\" >= 0");

                // Зовнішні ключі
                b.Property(s => s.DirectionId)
                    .IsRequired();

                b.Property(s => s.TypeId)
                    .IsRequired();

                // ============ ВІДНОШЕННЯ ============
                b.HasMany(s => s.Appointments)
                    .WithOne(a => a.Service)
                    .HasForeignKey(a => a.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(s => s.Reviews)
                    .WithOne(r => r.Service)
                    .HasForeignKey(r => r.ServiceId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Опціонально можна додати CheckConstraint на Name
                // b.HasCheckConstraint("CHK_Service_Name_Format", "\"Name\" ~ '^[A-Za-zА-Яа-я0-9\\s-]+$'");
            });



            // ============ APPOINTMENTS ============
            builder.Entity<Appointment>(b =>
            {
                b.ToTable("Appointments");

                // PRIMARY KEY
                b.HasKey(a => a.Id);

                // Зовнішні ключі
                b.Property(a => a.PatientId)
                    .IsRequired();

                b.Property(a => a.DoctorId)
                    .IsRequired();

                b.Property(a => a.ServiceId)
                    .IsRequired();

                // Дата та час призначення обов'язкові
                b.Property(a => a.AppointmentDate)
                    .IsRequired();

                // Статус обов'язковий, зберігаємо як string
                b.Property(a => a.Status)
                    .IsRequired()
                    .HasConversion<string>();

                // Дата створення, дефолт UTC
                b.Property(a => a.CreatedAt)
                    .HasDefaultValueSql("NOW()")
                    .IsRequired();

                // Наприклад, дата призначення не може бути в минулому
                b.HasCheckConstraint("CHK_Appointment_Date_Future", "\"AppointmentDate\" >= NOW()");
            });

            // ============ REVIEWS ============
            builder.Entity<Review>(b =>
            {
                b.ToTable("Reviews");

                // Поля
                b.Property(r => r.Rating)
                    .IsRequired()
                    .HasDefaultValue(1); // за замовчуванням мінімальна оцінка

                b.Property(r => r.Comment)
                    .HasMaxLength(1000);

                b.Property(r => r.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                // Індекси
                b.HasIndex(r => r.UserId);
                b.HasIndex(r => r.DoctorId);
                b.HasIndex(r => r.ServiceId);

                // Перевірка значень рейтингу (1-5)
                b.HasCheckConstraint("CHK_Review_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5");
            });



            // ============ SCHEDULE ============
            builder.Entity<Schedule>(entity =>
            {
                // Назва PK
                entity.HasKey(e => e.Id);

                // (Опціонально) Явна назва колонки для Id, якщо хочеш
                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                // Час початку / кінця — зберігаємо як SQL time
                entity.Property(e => e.StartTime)
                    .IsRequired()
                    .HasColumnType("time");

                entity.Property(e => e.EndTime)
                    .IsRequired()
                    .HasColumnType("time");

                // WeekDay — обмежимо довжину і дамо дефолт (за потреби)
                entity.Property(e => e.WeekDay)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnType("character varying");

                // Обмеження допустимих значень (приклад для українських назв)
                entity.HasCheckConstraint("CK_Schedules_WeekDay",
                    "\"WeekDay\" IN ('Понеділок','Вівторок','Середа','Четвер','П''ятниця','Субота','Неділя')");

                // Зв'язок зі Doctor (many schedules -> one doctor)
                entity.HasOne(e => e.Doctor)
                    .WithMany(d => d.Schedules)
                    .HasForeignKey(e => e.DoctorId)
                    // при видаленні доктора — видаляти розклад (cascade)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            // ============ MEDICAL RECORD ============
            builder.Entity<MedicalRecord>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Diagnosis)
                    .HasColumnType("text");

                entity.Property(e => e.Treatment)
                    .HasColumnType("text");

                entity.Property(e => e.Recommendations)
                    .HasColumnType("text");

                // Твоє нове поле
                entity.Property(e => e.Result)
                    .HasColumnType("text");

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Зв'язок з Patient
                entity.HasOne(e => e.Patient)
                    .WithMany(p => p.MedicalRecords)
                    .HasForeignKey(e => e.PatientId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                // Зв'язок з Doctor
                entity.HasOne(e => e.Doctor)
                    .WithMany(d => d.MedicalRecords)
                    .HasForeignKey(e => e.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                // Зв'язок з Service
                entity.HasOne(e => e.Service)
                    .WithMany(s => s.MedicalRecords) // якщо у Service є колекція записів
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.SetNull); // дозволяємо null, якщо сервіс видаляється
            });


        }
    }
}
