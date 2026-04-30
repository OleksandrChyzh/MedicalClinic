using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;
public static class ServiceTypeGenerator
{
    // Заміни DbContext на назву твого контексту, наприклад ClinicDbContext, 
    // або використовуй загальний DbContext, якщо це підходить твоїй архітектурі.
    public static async Task GenerateAsync(DbContext context)
    {
        // Якщо таблиця вже має записи - пропускаємо генерацію
        if (await context.Set<ServiceType>().AnyAsync())
        {
            return;
        }

        var serviceTypes = new List<ServiceType>
        {
            new() { Name = "Консультація", Description = "Первинний та вторинний прийом спеціаліста" },
            new() { Name = "Діагностика", Description = "Апаратні та лабораторні дослідження (УЗД, ЕКГ, аналізи)" },
            new() { Name = "Лікування", Description = "Процедури, терапія та оперативні втручання" }
        };

        await context.Set<ServiceType>().AddRangeAsync(serviceTypes);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Типи послуг (ServiceTypes) успішно згенеровано.");
    }
}
