using System.ComponentModel.DataAnnotations;

namespace BLL.Models.User
{
    public record Register
(
    [Required]
    [EmailAddress]
    string Email,

    [Required]
    string Password,

    [Required]
    string UserName,

    [Phone]
    string? PhoneNumber);
}
