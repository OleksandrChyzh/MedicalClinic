using Microsoft.EntityFrameworkCore;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using WebApp;
using DataGenerator;

var builder = WebApplication.CreateBuilder(args);

// 1. РЕЄСТРАЦІЯ СЕРВІСІВ
builder.Services.ConfigureServices(builder.Configuration);

// 2. НАЛАШТУВАННЯ SWAGGER
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(static options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Medical System API",
        Version = "v1",
        Description = "API для керування медичним центром (Пацієнти, Лікарі, Записи, Відгуки)"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введіть токен у форматі: Bearer {ваш_токен}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// 3. БЛОК ГЕНЕРАЦІЇ ДАНИХ ТА МІГРАЦІЙ
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // Міграції залишаємо, щоб база завжди була актуальною
        await context.Database.MigrateAsync();

        // ЗМІНЕНО: Коментуємо виклик сідерів, щоб не спамило в консоль і швидше запускалось
        //await DbSeeder.SeedAllAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "Критична помилка під час оновлення бази даних. Додаток зупинено.");
        throw;
    }
}

// 4. КОНФІГУРАЦІЯ HTTP-PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Medical API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
