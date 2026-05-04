using BLL.Interfaces;
using BLL.Services;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using BLL.MappingProfiles;
using DAL.Entities;
using DAL.Interfaces;
using DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer; // ДОДАНО
using Microsoft.IdentityModel.Tokens; // ДОДАНО
using System.Text; // ДОДАНО

namespace WebApp;

public static class ServiceExtensions
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("AppDbContext"));
        });

        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:4200") // Адреса твого Angular
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // Важливо, якщо будеш передавати токени або кукі
            });
        });

        // 1. РЕЄСТРАЦІЯ IDENTITY
        services.AddIdentity<User, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // 2. РЕЄСТРАЦІЯ JWT АВТЕНТИФІКАЦІЇ (НОВИЙ БЛОК)
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"];

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Для розробки можна false
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
            };
        });

        // 3. РЕЄСТРАЦІЯ UNIT OF WORK ТА ІНШИХ СЕРВІСІВ
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
