using Bogus;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;
public static class AppointmentGenerator
{
    public static async Task GenerateAsync(DbContext context)
    {
        if (await context.Set<Appointment>().AnyAsync())
        {
            return;
        }

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

        for (int i = 0; i < 50; i++)
        {
            var doctor = faker.PickRandom(doctors);
            var patient = faker.PickRandom(patients);
            var service = faker.PickRandom(services);

            // 1. ЗМІНА: Додаємо .ToUniversalTime() до базової дати Bogus
            var date = faker.Date.Future(1, DateTime.UtcNow.AddDays(1)).ToUniversalTime();

            // 2. ЗМІНА: Конструктор DateTime. Тут ти вже почав правильно.
            // Вказуючи DateTimeKind.Utc, ми кажемо Postgres: "Це UTC, не сварися".
            var appointmentDate = new DateTime(
                date.Year, date.Month, date.Day,
                faker.Random.Int(8, 17),
                faker.Random.Bool() ? 0 : 30,
                0, DateTimeKind.Utc);

            appointments.Add(new Appointment
            {
                PatientId = patient.Id,
                UserId = patient.UserId,
                DoctorId = doctor.Id,
                ServiceId = service.Id,
                AppointmentDate = appointmentDate,
                DurationMinutes = 30,
                Status = AppointmentStatus.CONFIRMED,
                // 3. ЗМІНА: Використовуй тільки UtcNow
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.Set<Appointment>().AddRangeAsync(appointments);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Записи на прийом ({appointments.Count} шт.) успішно згенеровано.");
    }
}
