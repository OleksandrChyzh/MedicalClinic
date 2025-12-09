using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Exceptions
{
    public class ManageIdentityException(string message) : Exception(message)
    {
        public static void Throw(IdentityResult result)
        {
            if (!result.Succeeded)
            {
                throw new ManageIdentityException(result.Errors.First().Description);
            }
        }
    }
}
