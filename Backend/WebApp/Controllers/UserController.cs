using BLL.Interfaces;
using BLL.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Всі ендпоінти тут вимагатимуть JWT токен
public class UserController(IUserService userService) : ControllerBase
{
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
