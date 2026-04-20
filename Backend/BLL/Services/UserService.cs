using System.Security.Claims;
using AutoMapper;
using BLL.Interfaces;
using BLL.Models.User;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services;
public class UserService(UserManager<User> userManager, IMapper mapper) : IUserService
{
    public async Task<GetUser> GetUserProfileAsync(ClaimsPrincipal userPrincipal)
    {
        // Identity автоматично шукає користувача за ClaimTypes.NameIdentifier з твого токена
        var user = await userManager.GetUserAsync(userPrincipal);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Користувача не знайдено");
        }

        return mapper.Map<GetUser>(user);
    }

    public async Task UpdateUserProfileAsync(UpdateUser dto, ClaimsPrincipal userPrincipal)
    {
        var user = await userManager.GetUserAsync(userPrincipal);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Користувача не знайдено");
        }

        // 1. Оновлюємо базові поля (UserName, PhoneNumber). 
        // AutoMapper проігнорує null-значення завдяки твоєму налаштуванню.
        mapper.Map(dto, user);

        // 2. Якщо користувач передав новий пароль, оновлюємо його окремо
        if (!string.IsNullOrEmpty(dto.Password))
        {
            // Видаляємо старий пароль і ставимо новий
            await userManager.RemovePasswordAsync(user);
            await userManager.AddPasswordAsync(user, dto.Password);
        }

        // 3. Зберігаємо зміни в базі
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            // Тут можеш використати свій ManageIdentityException.Throw(result);
            throw new Exception("Помилка при оновленні профілю: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
