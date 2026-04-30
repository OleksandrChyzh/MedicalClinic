using Bogus;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;
public static class PatientGenerator
{
    public static async Task GenerateAsync(DbContext context, UserManager<User> userManager)
    {
        // 1. Рахуємо, скільки пацієнтів вже є
        int currentCount = await context.Set<Patient>().CountAsync();
        int targetTotal = 100;

        if (currentCount >= targetTotal)
        {
            Console.WriteLine("✅ У базі вже 100 або більше пацієнтів.");
            return;
        }

        // 2. Отримуємо всіх користувачів з роллю "User"
        var patientUsers = await userManager.GetUsersInRoleAsync("User");

        // 3. Знаходимо тих користувачів, у яких ще ВЗАГАЛІ немає профілів пацієнтів
        var userIdsWithProfiles = await context.Set<Patient>().Select(p => p.UserId).Distinct().ToListAsync();
        var usersToProcess = patientUsers.Where(u => !userIdsWithProfiles.Contains(u.Id)).ToList();

        var faker = new Faker("uk");
        var patientsToAdd = new List<Patient>();

        foreach (var user in usersToProcess)
        {
            if (currentCount + patientsToAdd.Count >= targetTotal)
            {
                break;
            }

            // --- Дорослий (Власник акаунта) ---
            var adultGenderEnum = faker.PickRandom<Bogus.DataSets.Name.Gender>();
            var adultGenderString = adultGenderEnum == Bogus.DataSets.Name.Gender.Male ? "Male" : "Female";

            patientsToAdd.Add(new Patient
            {
                UserId = user.Id,
                FirstName = faker.Name.FirstName(adultGenderEnum),
                LastName = faker.Name.LastName(adultGenderEnum),
                BirthDate = faker.Date.Past(62, DateTime.UtcNow.AddYears(-18)).Date.ToUniversalTime(),
                Gender = adultGenderString
            });

            // --- Діти (30% шансу), якщо ліміт ще не вичерпано ---
            if (faker.Random.Bool(0.4f)) // Трохи збільшив шанс для швидшого набору 100
            {
                int kidsCount = faker.Random.Int(1, 2);
                for (int i = 0; i < kidsCount; i++)
                {
                    if (currentCount + patientsToAdd.Count >= targetTotal)
                    {
                        break;
                    }

                    var kidGenderEnum = faker.PickRandom<Bogus.DataSets.Name.Gender>();
                    patientsToAdd.Add(new Patient
                    {
                        UserId = user.Id,
                        FirstName = faker.Name.FirstName(kidGenderEnum),
                        LastName = faker.Name.LastName(kidGenderEnum),
                        BirthDate = faker.Date.Past(17, DateTime.UtcNow).Date.ToUniversalTime(),
                        Gender = kidGenderEnum == Bogus.DataSets.Name.Gender.Male ? "Male" : "Female"
                    });
                }
            }
        }

        if (patientsToAdd.Any())
        {
            await context.Set<Patient>().AddRangeAsync(patientsToAdd);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Догенеровано {patientsToAdd.Count} нових профілів пацієнтів. Загалом тепер: {currentCount + patientsToAdd.Count}");
        }
    }
}
