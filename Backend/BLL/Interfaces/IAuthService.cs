using BLL.Models.User;

namespace BLL.Interfaces;
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(Register dto);
    Task<AuthResponse> LoginAsync(Login dto);
}
