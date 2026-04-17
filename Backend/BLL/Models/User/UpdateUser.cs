using System.ComponentModel.DataAnnotations;

namespace BLL.Models.User
{
    public record UpdateUser(
    string? Password,
    string? UserName,
    [Phone]
    string? PhoneNumber);
}
