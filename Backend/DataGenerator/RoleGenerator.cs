using Microsoft.AspNetCore.Identity;

namespace DataGenerator;

public static class RoleGenerator
{
    public static async Task GenerateAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        var roles = new[] { "Admin", "Doctor", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        Console.WriteLine("✅ Ролі (Roles) успішно згенеровано.");
    }
}
