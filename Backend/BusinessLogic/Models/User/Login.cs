using System.ComponentModel.DataAnnotations;

namespace BLL.Models.User
{
    public record Login(
    [Required]
    [EmailAddress]
    string Email,

    [Required]
    string Password);
}
