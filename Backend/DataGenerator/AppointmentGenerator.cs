using Bogus;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;

public static class AppointmentGenerator
{
    public static async Task GenerateAsync(DbContext context)
    {
        // 1. Рахуємо, скільки записів уже є в базі
        var currentCount = await context.Set<Appointment>().CountAsync();
        var targetCount = 200;

        // Якщо в базі вже є 200 або більше записів, нічого не генеруємо
        if (currentCount >= targetCount)
        {
            Console.WriteLine($"✅ В базі вже є {currentCount} записів. Догенерація не потрібна.");
            return;
        }

        // Обчислюємо, скільки ще треба додати
        var neededCount = targetCount - currentCount;
        Console.WriteLine($"🔄 В базі знайдено {currentCount} записів. Генеруємо ще {neededCount} шт...");

        // 2. Отримуємо необхідні дані з бази
        var patients = await context.Set<Patient>().ToListAsync();
        var doctors = await context.Set<Doctor>().ToListAsync();
        var services = await context.Set<Service>().ToListAsync();

        if (!patients.Any() || !doctors.Any() || !services.Any())
        {
            Console.WriteLine("⚠️ Неможливо згенерувати записи: відсутні пацієнти, лікарі або послуги.");
            return;
        }

        var faker = new Faker("uk");
        var appointments = new List<Appointment>();

        // Масив доступних статусів
        var statuses = new[] {
            AppointmentStatus.CREATED,
            AppointmentStatus.CONFIRMED,
            AppointmentStatus.COMPLETED,
            AppointmentStatus.CANCELLED
        };

        // 3. Генеруємо рівно стільки записів, скільки не вистачає до 200
        for (int i = 0; i < neededCount; i++)
        {
            var doctor = faker.PickRandom(doctors);
            var patient = faker.PickRandom(patients);
            var service = faker.PickRandom(services);
            var status = faker.PickRandom(statuses);

            DateTime randomDate;

            // Логіка залежності дати від статусу
            if (status == AppointmentStatus.COMPLETED || status == AppointmentStatus.CANCELLED)
            {
                // Якщо завершено або скасовано -> дата в МИНУЛОМУ (наприклад, за останні 60 днів)
                randomDate = faker.Date.Recent(60);
            }
            else
            {
                // Якщо очікує або підтверджено -> дата в МАЙБУТНЬОМУ (максимум до 30 днів вперед)
                randomDate = DateTime.Now.AddDays(faker.Random.Int(1, 30));
            }

            // Формуємо фінальну дату прийому
            var appointmentDate = new DateTime(
                randomDate.Year,
                randomDate.Month,
                randomDate.Day,
                faker.Random.Int(8, 17),       // Робочі години з 08:00 до 17:00
                faker.Random.Bool() ? 0 : 30,  // Прийоми по 30 хвилин
                0,
                DateTimeKind.Unspecified);

            appointments.Add(new Appointment
            {
                PatientId = patient.Id,
                UserId = patient.UserId,
                DoctorId = doctor.Id,
                ServiceId = service.Id,
                AppointmentDate = appointmentDate,
                DurationMinutes = 30,
                Status = status,

                CreatedAt = DateTime.SpecifyKind(DateTime.Now.AddDays(-faker.Random.Int(1, 10)), DateTimeKind.Unspecified)
            });
        }

        await context.Set<Appointment>().AddRangeAsync(appointments);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Успішно догенеровано {appointments.Count} записів. Тепер у базі загалом {currentCount + appointments.Count} записів.");
    }
}
