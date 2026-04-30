using Microsoft.EntityFrameworkCore;
using DAL.Data;
using DAL.Entities; // Твій User
using Microsoft.AspNetCore.Identity;
using WebApp; // Для доступу до ServiceExtensions
using DataGenerator; // Проект з твоїм сідером

var builder = WebApplication.CreateBuilder(args);

// Викликаємо твій метод конфігурації (щоб не дублювати код тут і в ServiceExtensions)
builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();

// ==========================================
// БЛОК ГЕНЕРАЦІЇ ДАНИХ (SEEDING)
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        await context.Database.MigrateAsync();
        await DbSeeder.SeedAllAsync(context, userManager, roleManager);
    }
    // Міняємо Exception на більш конкретні, або логіюємо і прокидаємо далі
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "Критична помилка під час заповнення бази даних. Додаток зупинено.");

        // ВАЖЛИВО: Перекидаємо помилку далі. 
        // Якщо база не заповнилася, програма не повинна працювати.
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
