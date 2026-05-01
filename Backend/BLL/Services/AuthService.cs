using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using BLL.Interfaces;
using BLL.Models.User;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Services;
public class AuthService(
    UserManager<User> userManager,
    IMapper mapper,
    IConfiguration configuration) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(Register dto)
    {
        var user = mapper.Map<User>(dto);
        user.UserName = dto.UserName; // Identity вимагає UserName
        _ = await userManager.CreateAsync(user, dto.Password);

        // Твій кастомний обробник помилок, про який ти згадував

        // Призначаємо роль за замовчуванням
        await userManager.AddToRoleAsync(user, "User");

        return new AuthResponse
        {
            Token = await this.GenerateJwtToken(user),
            Roles = new[] { "User" }
        };
    }

    public async Task<AuthResponse> LoginAsync(Login dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);

        if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
        {
            throw new UnauthorizedAccessException("Невірний email або пароль");
        }

        var roles = await userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            Token = await this.GenerateJwtToken(user),
            Roles = roles
        };
    }

    private async Task<string> GenerateJwtToken(User user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.UserName!),
            new(JwtRegisteredClaimNames.Sub, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
