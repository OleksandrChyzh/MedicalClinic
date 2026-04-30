using Bogus;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;

public static class ScheduleGenerator
{
    private static readonly string[] DaysOfWeek =
    [
        "Понеділок", "Вівторок", "Середа", "Четвер", "П'ятниця", "Субота", "Неділя"
    ];

    public static async Task GenerateAsync(DbContext context)
    {
        // 1. Отримуємо ID всіх лікарів
        var allDoctorIds = await context.Set<Doctor>().Select(d => d.Id).ToListAsync();

        // 2. Отримуємо ID лікарів, для яких розклад ВЖЕ існує
        var existingDoctorIdsWithSchedule = await context.Set<Schedule>()
            .Select(s => s.DoctorId)
            .Distinct()
            .ToListAsync();

        // 3. Відфільтровуємо тільки тих, у кого розкладу ще немає
        var doctorsToProcess = allDoctorIds.Where(id => !existingDoctorIdsWithSchedule.Contains(id)).ToList();

        if (!doctorsToProcess.Any())
        {
            Console.WriteLine("✅ У всіх лікарів уже є сформований розклад.");
            return;
        }

        var faker = new Faker("uk");
        var schedulesToAdd = new List<Schedule>();

        foreach (var doctorId in doctorsToProcess)
        {
            // Твоя логіка: від 3 до 6 робочих днів
            int daysCount = faker.Random.Int(3, 6);
            var workingDays = faker.PickRandom(DaysOfWeek, daysCount).ToList();

            foreach (var day in workingDays)
            {
                // Твоя логіка часу: без змін
                int startHour = faker.Random.Int(8, 11);
                int endHour = faker.Random.Int(15, 18);

                schedulesToAdd.Add(new Schedule
                {
                    DoctorId = doctorId,
                    WeekDay = day,
                    StartTime = new TimeOnly(startHour, 0),
                    EndTime = new TimeOnly(endHour, 0)
                });
            }
        }

        if (schedulesToAdd.Any())
        {
            await context.Set<Schedule>().AddRangeAsync(schedulesToAdd);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Догенеровано розклад для {doctorsToProcess.Count} нових лікарів ({schedulesToAdd.Count} записів).");
        }
    }
}
