using Bogus;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;

public static class ServiceGenerator
{
    public static async Task GenerateAsync(DbContext context)
    {
        // 1. Отримуємо всі напрямки та типи послуг
        var allDirections = await context.Set<Direction>().ToListAsync();
        var allTypes = await context.Set<ServiceType>().ToListAsync();

        if (!allDirections.Any() || !allTypes.Any())
        {
            Console.WriteLine("⚠️ Неможливо згенерувати послуги: відсутні довідники напрямків або типів.");
            return;
        }

        // 2. Рахуємо, скільки послуг уже є для кожного напрямку
        var existingCounts = await context.Set<Service>()
            .GroupBy(s => s.DirectionId)
            .Select(g => new { DirectionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DirectionId, x => x.Count);

        var faker = new Faker("uk");
        var servicesToAdd = new List<Service>();

        // Словник ключових слів для генерації назв за напрямками
        var medicalKeywords = new Dictionary<string, string[]>
        {
            ["Терапія"] = ["Обстеження", "Діагностика", "Аналіз стану", "Профільний огляд", "Довідка для", "Комплекс"],
            ["Кардіологія"] = ["Ехо-скринінг", "Моніторинг", "Тест на навантаження", "Аналіз ритму", "Кардіо-пакет", "Оцінка судин"],
            ["Дерматологія"] = ["Аналіз пігментації", "Кріотерапія", "Видалення", "Очищення", "Лікування", "Фототерапія"],
            ["Неврологія"] = ["Рефлексотерапія", "Блокада", "Обстеження нервів", "ЕЕГ", "Стимуляція", "Контроль"],
            ["Хірургія"] = ["Маніпуляція", "Обробка", "Видалення", "Операція", "Корекція", "Пункція"],
            ["Педіатрія"] = ["Дитячий огляд", "Патронаж", "Вакцинація", "Скринінг росту", "Аналіз розвитку", "Шкільна довідка"]
        };

        // Додаткові прикметники для унікальності
        string[] adjectives = { "розширений", "комплексний", "первинний", "терміновий", "експертний", "стандартний", "плановий" };

        foreach (var direction in allDirections)
        {
            int currentCount = existingCounts.ContainsKey(direction.Id) ? existingCounts[direction.Id] : 0;
            int target = 20;
            int toGenerate = target - currentCount;

            if (toGenerate <= 0)
            {
                continue;
            }

            // Визначаємо набір слів для цього напрямку (або дефолтний)
            var keywords = medicalKeywords.ContainsKey(direction.Name)
                ? medicalKeywords[direction.Name]
                : new[] { "Консультація", "Процедура", "Послуга" };

            for (int i = 0; i < toGenerate; i++)
            {
                var type = faker.PickRandom(allTypes);
                var adj = faker.PickRandom(adjectives);
                var keyword = faker.PickRandom(keywords);

                // Формуємо назву (наприклад: "Експертний Ехо-скринінг")
                string serviceName = $"{char.ToUpper(adj[0]) + adj[1..]} {keyword}";

                // Додаємо випадковий номер або підзаголовок, щоб уникнути дублікатів назв
                if (i % 3 == 0)
                {
                    serviceName += " (тип А)";
                }

                if (i % 5 == 0)
                {
                    serviceName += " категорії " + faker.Random.Int(1, 3);
                }

                servicesToAdd.Add(new Service
                {
                    Name = serviceName,
                    Description = $"{keyword} у відділенні {direction.Name.ToLower()}. {faker.Lorem.Sentence(5)}",
                    Price = faker.Random.Decimal(300, 3500), // Ціни від 300 до 3500 грн
                    DirectionId = direction.Id,
                    TypeId = type.Id
                });
            }
        }

        if (servicesToAdd.Any())
        {
            await context.Set<Service>().AddRangeAsync(servicesToAdd);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Догенеровано {servicesToAdd.Count} нових послуг (по 20 на кожен напрямок).");
        }
        else
        {
            Console.WriteLine("✅ Всі напрямки вже мають по 20 послуг.");
        }
    }
}
