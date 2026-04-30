using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;
public static class DirectionGenerator
{
    public static async Task GenerateAsync(DbContext context)
    {
        // Перевіряємо чи існують напрямки
        if (await context.Set<Direction>().AnyAsync())
        {
            return;
        }

        var directions = new List<Direction>
        {
            new() { Name = "Терапія", Description = "Діагностика та нехірургічне лікування захворювань внутрішніх органів" },
            new() { Name = "Кардіологія", Description = "Профілактика, діагностика та лікування серцево-судинних захворювань" },
            new() { Name = "Дерматологія", Description = "Діагностика та лікування захворювань шкіри, волосся та нігтів" },
            new() { Name = "Неврологія", Description = "Лікування захворювань центральної та периферичної нервової системи" },
            new() { Name = "Хірургія", Description = "Діагностика та лікування захворювань, що потребують оперативного втручання" },
            new() { Name = "Педіатрія", Description = "Збереження здоров'я дітей, діагностика та лікування дитячих хвороб" }
        };

        await context.Set<Direction>().AddRangeAsync(directions);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Напрямки (Directions) успішно згенеровано.");
    }
}
