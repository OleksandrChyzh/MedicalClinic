namespace BLL.Models.User;
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public IEnumerable<string> Roles { get; set; } = [];
}
