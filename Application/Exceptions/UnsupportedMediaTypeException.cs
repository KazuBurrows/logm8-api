namespace LogMate.Application.Exceptions;

public class UnsupportedMediaTypeException : Exception
{
    public UnsupportedMediaTypeException(string message) : base(message)
    {
    }
}
