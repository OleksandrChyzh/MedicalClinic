using Microsoft.EntityFrameworkCore;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using WebApp;
using DataGenerator;

var builder = WebApplication.CreateBuilder(args);

// 1. РЕЄСТРАЦІЯ СЕРВІСІВ (BLL, DAL, Identity, AutoMapper)
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

    // Додаємо підтримку JWT токенів у інтерфейсі Swagger
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

// 3. БЛОК ГЕНЕРАЦІЇ ДАНИХ (SEEDING)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // Автоматичне застосування міграцій при старті
        await context.Database.MigrateAsync();

        // Запуск твоїх генераторів
        await DbSeeder.SeedAllAsync(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "Критична помилка під час заповнення бази даних. Додаток зупинено.");
        throw;
    }
}

// 4. КОНФІГУРАЦІЯ HTTP-PIPELINE (Middleware)
if (app.Environment.IsDevelopment())
{
    // Включаємо генерацію специфікації та UI
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Medical API v1");
        options.RoutePrefix = "swagger"; // Swagger буде доступний за адресою /swagger
    });
}

app.UseHttpsRedirection();

// Важливо: Authentication ЗАВЖДИ перед Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
