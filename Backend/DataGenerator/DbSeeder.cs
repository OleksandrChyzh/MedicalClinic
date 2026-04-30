using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataGenerator;
public static class DbSeeder
{
    public static async Task SeedAllAsync(
        DbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        Console.WriteLine("🚀 Запуск процесу генерації даних...");

        // --- ЕТАП 1: Базові словники (Незалежні) ---
        await RoleGenerator.GenerateAsync(roleManager);
        await ServiceTypeGenerator.GenerateAsync(context);
        await DirectionGenerator.GenerateAsync(context);

        // --- ЕТАП 2: Словники із залежностями ---
        // Залежить від Directions та ServiceTypes
        await ServiceGenerator.GenerateAsync(context);

        // --- ЕТАП 3: Акаунти користувачів (Identity) ---
        // Залежить від Roles
        await UserGenerator.GenerateAsync(userManager);

        // --- ЕТАП 4: Профілі (Медичний персонал та Клієнти) ---
        // Залежать від Users та Directions
        await DoctorGenerator.GenerateAsync(context, userManager);
        await PatientGenerator.GenerateAsync(context, userManager);

        // --- ЕТАП 5: Робочі процеси та Історія ---
        // Залежить від Doctors
        await ScheduleGenerator.GenerateAsync(context);

        // Залежить від Patients, Doctors, Services
        await AppointmentGenerator.GenerateAsync(context);

        // Залежить від Patients та Doctors
        await MedicalRecordGenerator.GenerateAsync(context);

        // Залежить від Users, Doctors, Services
        await ReviewGenerator.GenerateAsync(context);

        Console.WriteLine("🏁 Генерація успішно завершена!");
    }
}
