using Bogus;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;

public static class AppointmentGenerator
{
    public static async Task GenerateAsync(DbContext context)
    {
        // 1. Перевірка: якщо записи вже є, нічого не робимо
        if (await context.Set<Appointment>().AnyAsync())
        {
            return;
        }

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

        // Генеруємо 50 записів
        for (int i = 0; i < 50; i++)
        {
            var doctor = faker.PickRandom(doctors);
            var patient = faker.PickRandom(patients);
            var service = faker.PickRandom(services);

            // Генеруємо випадкову дату в майбутньому (протягом наступного року)
            var futureDate = faker.Date.Future(1, DateTime.Now.AddDays(1));

            // Формуємо фінальну дату прийому:
            // Використовуємо DateTimeKind.Unspecified для коректної роботи з 'timestamp without time zone'
            var appointmentDate = new DateTime(
                futureDate.Year,
                futureDate.Month,
                futureDate.Day,
                faker.Random.Int(8, 17),       // Робочі години з 08:00 до 17:00
                faker.Random.Bool() ? 0 : 30,  // Прийоми по 30 хвилин (наприклад, 10:00 або 10:30)
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
                Status = AppointmentStatus.CONFIRMED,

                // Для дати створення також використовуємо локальний час без часового поясу
                CreatedAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
            });
        }

        await context.Set<Appointment>().AddRangeAsync(appointments);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Записи на прийом ({appointments.Count} шт.) успішно згенеровано (без часових поясів).");
    }
}
