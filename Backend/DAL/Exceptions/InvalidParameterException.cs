namespace DAL.Exceptions;

public class InvalidParameterException : Exception
{
    public InvalidParameterException(string message)
        : base(message)
    {
    }

    public InvalidParameterException()
        : base()
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
