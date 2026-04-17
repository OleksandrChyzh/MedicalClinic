namespace BLL.Models.User
{
    public record GetUser(
    int Id,
    string Email,
    string UserName,
    string? PhoneNumber);
}
