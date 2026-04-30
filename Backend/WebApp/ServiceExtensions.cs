using BLL.Interfaces;
using BLL.Services;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using BLL.MappingProfiles;
using DAL.Entities;
using DAL.Interfaces;
using DAL;
using Microsoft.AspNetCore.Identity;

namespace WebApp;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("AppDbContext"));
        });

        // 2. РЕЄСТРАЦІЯ IDENTITY (Цього не вистачало для UserManager)
        services.AddIdentity<User, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = false; // Налаштування за бажанням
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // 3. РЕЄСТРАЦІЯ UNIT OF WORK (Цього не вистачало для BLL сервісів)
        // Заміни UnitOfWork на назву свого класу реалізації, якщо вона інша
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddControllers();
        services.AddAutoMapper(config =>
        {
            config.AddMaps(typeof(UserProfile).Assembly);
        });
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDirectionService, DirectionService>();
        services.AddScoped<IServiceManagementService, ServiceManagementService>();
        services.AddScoped<IServiceTypeService, ServiceTypeService>();
        services.AddScoped<IMedicalRecordService, MedicalRecordService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IReviewService, ReviewService>();
    }
}
