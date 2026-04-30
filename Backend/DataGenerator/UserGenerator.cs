using Bogus;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace DataGenerator;
public static class UserGenerator
{
    public static async Task GenerateAsync(UserManager<User> userManager)
    {
        Console.WriteLine("⏳ Перевірка та генерація користувачів (Users)...");

        // 1. СТВОРЕННЯ АДМІНІСТРАТОРА (Тільки якщо його ще немає)
        var admins = await userManager.GetUsersInRoleAsync("Admin");
        if (!admins.Any())
        {
            var adminUser = new User
            {
                UserName = "admin@clinic.com",
                Email = "admin@clinic.com",
                PhoneNumber = "+380000000000",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var adminResult = await userManager.CreateAsync(adminUser, "AdminPass123!");
            if (adminResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("✅ Адміністратора створено.");
            }
        }

        var faker = new Faker("uk");
        string defaultPassword = "TestPassword123!";

        // 2. ДОГЕНЕРОВУЄМО ЛІКАРІВ (Ціль: 40 штук)
        var currentDoctors = await userManager.GetUsersInRoleAsync("Doctor");
        int targetDoctorsCount = 40;
        int doctorsToGenerate = targetDoctorsCount - currentDoctors.Count;

        if (doctorsToGenerate > 0)
        {
            for (int i = 0; i < doctorsToGenerate; i++)
            {
                var doctorUser = new User
                {
                    UserName = faker.Internet.UserName().ToLower(),
                    Email = faker.Internet.Email().ToLower(),
                    PhoneNumber = faker.Phone.PhoneNumber("+380#########")
                };

                var result = await userManager.CreateAsync(doctorUser, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctorUser, "Doctor");
                }
            }
            Console.WriteLine($"✅ Догенеровано акаунти лікарів: {doctorsToGenerate} шт.");
        }

        // 3. ДОГЕНЕРОВУЄМО ПАЦІЄНТІВ (Ціль: 30 штук, за бажанням можеш змінити)
        var currentPatients = await userManager.GetUsersInRoleAsync("User");
        int targetPatientsCount = 70;
        int patientsToGenerate = targetPatientsCount - currentPatients.Count;

        if (patientsToGenerate > 0)
        {
            for (int i = 0; i < patientsToGenerate; i++)
            {
                var patientUser = new User
                {
                    UserName = faker.Internet.UserName().ToLower(),
                    Email = faker.Internet.Email().ToLower(),
                    PhoneNumber = faker.Phone.PhoneNumber("+380#########")
                };

                var result = await userManager.CreateAsync(patientUser, defaultPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(patientUser, "User");
                }
            }
            Console.WriteLine($"✅ Догенеровано акаунти пацієнтів: {patientsToGenerate} шт.");
        }
    }
}
