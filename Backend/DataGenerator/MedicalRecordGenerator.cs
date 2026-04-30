using Bogus;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;

public static class MedicalRecordGenerator
{
    public static async Task GenerateAsync(DbContext context)
    {
        // 1. Перевірка кількості. Якщо вже є 200, не дублюємо.
        int currentCount = await context.Set<MedicalRecord>().CountAsync();
        int targetTotal = 200;

        if (currentCount >= targetTotal)
        {
            Console.WriteLine($"✅ У базі вже є {currentCount} медичних записів.");
            return;
        }

        // Завантажуємо лікарів разом із їхніми напрямками
        var doctors = await context.Set<Doctor>().Include(d => d.Direction).ToListAsync();
        var patients = await context.Set<Patient>().ToListAsync();
        var services = await context.Set<Service>().ToListAsync();

        if (!patients.Any() || !doctors.Any() || !services.Any())
        {
            Console.WriteLine("⚠️ Неможливо згенерувати MedicalRecords: перевірте наявність пацієнтів, лікарів та послуг.");
            return;
        }

        var faker = new Faker("uk");
        var recordsToAdd = new List<MedicalRecord>();

        // СЛОВНИК МЕДИЧНИХ ДАНИХ (Для аналітики)
        // Ключ — назва напрямку (має збігатися з тим, що в базі)
        var medicalAtlas = new Dictionary<string, (string[] Diagnoses, string[] Treatments, string[] Recs)>
        {
            ["Кардіологія"] = (
                new[] { "Гіпертонічна хвороба II ступеня", "Ішемічна хвороба серця", "Порушення ритму (аритмія)", "Вегето-судинна дистонія" },
                new[] { "Прийом бета-блокаторів", "Курс препаратів магнію", "Терапія інгібіторами АПФ" },
                new[] { "Щоденний контроль артеріального тиску", "Обмеження вживання солі", "Відмова від кави" }
            ),
            ["Дерматологія"] = (
                new[] { "Атопічний дерматит", "Псоріаз звичайний", "Контактний дерматит", "Себорея" },
                new[] { "Місцеві кортикостероїди", "Курс антигістамінних препаратів", "Фототерапія" },
                new[] { "Використання емолієнтів", "Гіпоалергенна дієта", "Уникати агресивних миючих засобів" }
            ),
            ["Неврологія"] = (
                new[] { "Мігрень з аурою", "Остеохондроз хребта", "Міжреберна невралгія", "Синдром хронічної втоми" },
                new[] { "Мануальна терапія", "Прийом нестероїдних протизапальних засобів", "Вітаміни групи B" },
                new[] { "Лікувальна фізкультура", "Корекція режиму сну", "Курс масажу" }
            ),
            ["Гастроентерологія"] = (
                new[] { "Хронічний гастрит", "Виразкова хвороба шлунка", "Холецистит", "Синдром подразненого кишечника" },
                new[] { "Прийом антацидів", "Ферментна терапія", "Антибактеріальна терапія (H.pylori)" },
                new[] { "Дієта №5", "Дробове харчування 5-6 разів на день", "Уникати смаженого та гострого" }
            )
        };

        (string[] Diagnoses, string[] Treatments, string[] Recs) defaultData = (
    new[] { "Загальне нездужання", "ГРВІ", "Профілактичний огляд" },
    new[] { "Симптоматичне лікування", "Вітамінотерапія" },
    new[] { "Рясне пиття", "Повторний огляд через тиждень" }
);

        int recordsToCreate = targetTotal - currentCount;

        for (int i = 0; i < recordsToCreate; i++)
        {
            var doctor = faker.PickRandom(doctors);
            var patient = faker.PickRandom(patients);
            var service = faker.PickRandom(services);

            var directionName = doctor.Direction?.Name ?? "Загальний";

            // Отримуємо кортеж. Якщо імена все одно "відваляться", використаємо Item1, Item2...
            var data = medicalAtlas.ContainsKey(directionName) ? medicalAtlas[directionName] : defaultData;

            // ВИПРАВЛЕНО: Використовуємо Item1, Item2, Item3 — це 100% залізобетонно працює в будь-якій версії C#
            var diagnosis = faker.PickRandom(data.Item1);    // Це Diagnoses
            var treatment = faker.PickRandom(data.Item2);    // Це Treatments
            var recommendation = faker.PickRandom(data.Item3); // Це Recs

            recordsToAdd.Add(new MedicalRecord
            {
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                ServiceId = service.Id,
                Diagnosis = diagnosis, // Твій діагноз
                Result = "Обстеження проведено. Спостерігається помірна динаміка.",
                Treatment = treatment,
                Recommendations = recommendation,
                CreatedAt = DateTime.SpecifyKind(faker.Date.Past(1), DateTimeKind.Unspecified)
            });
        }

        await context.Set<MedicalRecord>().AddRangeAsync(recordsToAdd);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Успішно догенеровано {recordsToAdd.Count} медичних карток українською мовою.");
    }
}
