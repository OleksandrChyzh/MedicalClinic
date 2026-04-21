using BLL.Interfaces;
using BLL.Services;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using BLL.MappingProfiles;

namespace WebApp;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("AppDbContext"));
        });

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


    }
}
