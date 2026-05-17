using BLL.Interfaces;
using BLL.Models.User;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Всі ендпоінти тут вимагатимуть JWT токен
public class UserController(IUserService userService, UserManager<User> userManager) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult GetAll()
    {
        var users = userManager.Users
            .OrderBy(u => u.Id)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.UserName,
                u.PhoneNumber,
                IsBlocked = u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow
            })
            .ToList();

        return this.Ok(users);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/block")]
    public async Task<IActionResult> SetBlockStatus(int id, [FromQuery] bool blocked)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return this.NotFound();
        }

        user.LockoutEnabled = true;
        user.LockoutEnd = blocked ? DateTimeOffset.UtcNow.AddYears(100) : null;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return this.BadRequest(result.Errors);
        }

        return this.NoContent();
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        // User - це вбудована властивість ControllerBase (тип ClaimsPrincipal)
        var profile = await userService.GetUserProfileAsync(this.User);
        return this.Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUser dto)
    {
        await userService.UpdateUserProfileAsync(dto, this.User);
        return this.NoContent(); // 204 No Content - стандартна відповідь при успішному оновленні
    }
}
