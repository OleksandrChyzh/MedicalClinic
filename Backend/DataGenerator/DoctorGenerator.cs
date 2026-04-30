using Bogus;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;
public static class DoctorGenerator
{
    public static async Task GenerateAsync(DbContext context, UserManager<User> userManager)
    {
        // Отримуємо всі напрямки як ОБ'ЄКТИ, бо нам потрібна їхня назва для опису
        var directions = await context.Set<Direction>().ToListAsync();
        if (!directions.Any())
        {
            Console.WriteLine("⚠️ Неможливо згенерувати лікарів: відсутні напрямки (Directions).");
            return;
        }

        // Отримуємо всіх користувачів з роллю "Doctor"
        var doctorUsers = await userManager.GetUsersInRoleAsync("Doctor");

        // Отримуємо ID тих користувачів, які ВЖЕ мають створений профіль лікаря
        var existingDoctorUserIds = await context.Set<Doctor>().Select(d => d.UserId).ToListAsync();

        // Відфільтровуємо лише тих, кому треба створити профіль
        var usersWithoutProfile = doctorUsers.Where(u => !existingDoctorUserIds.Contains(u.Id)).ToList();

        if (!usersWithoutProfile.Any())
        {
            return;
        }

        var faker = new Faker("uk");
        var doctors = new List<Doctor>();

        // Шаблони для описів. {0} буде замінено на назву напрямку.
        string[] descriptionTemplates =
        [
            "Досвідчений фахівець за напрямком '{0}'. Надає кваліфіковану медичну допомогу, використовуючи сучасні методи доказової медицини. Постійно вдосконалює свої навички на міжнародних конференціях та семінарах.",
            "Лікар вищої категорії у сфері '{0}'. Спеціалізується на складних клінічних випадках, забезпечуючи індивідуальний підхід до кожного пацієнта. Працює виключно з найсучаснішим діагностичним обладнанням.",
            "Провідний спеціаліст клініки. Напрямок роботи: {0}. Має багаторічний досвід успішного лікування та тисячі вдячних пацієнтів. Головний принцип роботи – турбота, емпатія та максимальна увага до деталей здоров'я пацієнта.",
            "Експерт з напрямку '{0}'. Проводить комплексну діагностику та призначає максимально ефективне і безпечне лікування. Відзначається високим рівнем професіоналізму та відповідальності."
        ];

        foreach (var user in usersWithoutProfile)
        {
            var gender = faker.PickRandom<Bogus.DataSets.Name.Gender>();
            var direction = faker.PickRandom(directions); // Вибираємо випадковий напрямок

            // Формуємо красивий український опис, підставляючи назву напрямку в нижньому регістрі
            string description = string.Format(
                faker.PickRandom(descriptionTemplates),
                direction.Name.ToLower()
            );

            var doctor = new Doctor
            {
                UserId = user.Id,
                DirectionId = direction.Id, // Беремо ID з вибраного напрямку
                FirstName = faker.Name.FirstName(gender),
                LastName = faker.Name.LastName(gender),
                MiddleName = faker.Name.FirstName(gender) + "ович", // Імітація по батькові
                ExperienceYears = faker.Random.Int(0, 35),
                Description = description
            };

            // Перестраховка: обрізаємо опис, якщо він раптом перевищить ліміт 1000 символів
            if (doctor.Description.Length > 1000)
            {
                doctor.Description = doctor.Description[..997] + "...";
            }

            doctors.Add(doctor);
        }

        await context.Set<Doctor>().AddRangeAsync(doctors);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Догенеровано нових профілів лікарів: {doctors.Count} шт.");
    }
}
