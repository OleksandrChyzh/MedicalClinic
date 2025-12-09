using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Exceptions
{
    public class InvalidParameterException : Exception
    {
        public InvalidParameterException(string message)
            : base(message)
        {
        }

        public InvalidParameterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public static void ThrowIfInvalidRange<T>(T min, T max)
            where T : IComparable<T>
        {
            if (min.CompareTo(max) > 0)
            {
                throw new InvalidParameterException($"Invalid range: {min} - {max}");
            }
        }
    }
}
