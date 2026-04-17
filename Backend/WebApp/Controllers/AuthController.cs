using BLL.Interfaces;
using BLL.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Register dto)
    {
        var result = await authService.RegisterAsync(dto);
        return this.Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login dto)
    {
        var result = await authService.LoginAsync(dto);
        return this.Ok(result);
    }
}
