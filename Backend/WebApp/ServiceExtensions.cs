using DAL.Data;
using Microsoft.EntityFrameworkCore;

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
    }
}
