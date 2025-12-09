using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Exceptions
{
    public class NotFoundException(string message) : Exception(message)
    {
        public NotFoundException(string entityName, string propertyName, string propertyValue)
            : this($"{entityName} with {propertyName} = '{propertyValue}' not found")
        {
        }
    }
}