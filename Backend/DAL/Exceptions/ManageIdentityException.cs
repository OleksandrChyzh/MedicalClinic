using Microsoft.AspNetCore.Identity;

namespace DAL.Exceptions;

public class ManageIdentityException(string message) : Exception(message)
{
    public ManageIdentityException() : this("An identity error occurred.")
    {
    }

    public ManageIdentityException(string message, Exception innerException)
        : this(message)
    {
    }

    public static void Throw(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new ManageIdentityException(result.Errors.First().Description);
        }
    }
}
