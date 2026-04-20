using System.Security.Claims;
using BLL.Models.User;

namespace BLL.Interfaces;
public interface IUserService
{
    Task<GetUser> GetUserProfileAsync(ClaimsPrincipal userPrincipal);
    Task UpdateUserProfileAsync(UpdateUser dto, ClaimsPrincipal userPrincipal);
}
