namespace DAL.Exceptions;

public class NotFoundException(string message) : Exception(message)
{
    public NotFoundException()
        : this("The requested resource was not found.")
    {
    }

    public NotFoundException(string entityName, string propertyName, string propertyValue)
        : this($"{entityName} with {propertyName} = '{propertyValue}' not found")
    {
    }

    public NotFoundException(string message, Exception innerException)
        : this(message)
    {
    }
}
