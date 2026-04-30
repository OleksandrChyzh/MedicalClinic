using Bogus;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;

public static class ReviewGenerator
{
    public static async Task GenerateAsync(DbContext context)
    {
        // 1. Перевірка кількості. Якщо вже є 100, не дублюємо.
        int currentCount = await context.Set<Review>().CountAsync();
        int targetTotal = 100;

        if (currentCount >= targetTotal)
        {
            Console.WriteLine($"✅ У базі вже є {currentCount} відгуків.");
            return;
        }

        var userIds = await context.Set<Patient>().Select(p => p.UserId).Distinct().ToListAsync();
        var doctors = await context.Set<Doctor>().ToListAsync();
        var services = await context.Set<Service>().ToListAsync();

        if (!userIds.Any() || !doctors.Any() || !services.Any())
        {
            Console.WriteLine("⚠️ Пропуск MedicalRecords: недостатньо даних (пацієнти, лікарі або послуги).");
            return;
        }

        var faker = new Faker("uk");
        var reviewsToAdd = new List<Review>();

        // Словник фраз для реалістичних відгуків
        string[] positiveComments = {
            "Дуже вдячний за професійний підхід! Лікар уважно вислухав і допоміг.",
            "Чудова клініка і дуже приємний лікар. Прийом пройшов комфортно.",
            "Рекомендую! Все чітко, швидко і по справі. Справжні професіонали.",
            "Лікар від Бога! Дуже задоволений результатом лікування.",
            "Приємно вражений сервісом. Все на вищому рівні.",
            "Найкращий фахівець, у якого я був. Тепер тільки сюди.",
            "Дякую за людяність та професіоналізм. Дуже допоміг ваш прийом."
        };

        string[] neutralComments = {
            "Прийом пройшов нормально, але довелося трохи почекати в черзі.",
            "Лікар хороший, але ціни в клініці кусаються.",
            "В цілому задоволений, хоча консультація була дуже швидкою.",
            "Звичайний прийом, нічого особливого. Лікування призначили."
        };

        string[] negativeComments = {
            "Не дуже сподобалося ставлення персоналу на рецепції.",
            "Чекав свого запису 20 хвилин. Лікар постійно відволікався.",
            "Очікував більшого від консультації за такі гроші."
        };

        int countToCreate = targetTotal - currentCount;

        for (int i = 0; i < countToCreate; i++)
        {
            var rating = faker.Random.Int(1, 5);
            string comment;

            // Вибираємо коментар залежно від оцінки
            if (rating >= 4)
            {
                comment = faker.PickRandom(positiveComments);
            }
            else if (rating == 3)
            {
                comment = faker.PickRandom(neutralComments);
            }
            else
            {
                comment = faker.PickRandom(negativeComments);
            }

            reviewsToAdd.Add(new Review
            {
                UserId = faker.PickRandom(userIds),
                DoctorId = faker.PickRandom(doctors).Id,
                ServiceId = faker.PickRandom(services).Id,
                Rating = rating,
                Comment = comment,
                // Залишаємо формат дати як ти просив (якщо працює - не чіпаємо)
                CreatedAt = faker.Date.Past(1, DateTime.Now)
            });
        }

        await context.Set<Review>().AddRangeAsync(reviewsToAdd);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Відгуки успішно догенеровано. Всього в базі: {targetTotal} шт.");
    }
}
