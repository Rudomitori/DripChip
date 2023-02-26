namespace Common.Domain.Exceptions;

public class InternalException : DomainExceptionBase
{
    public InternalException(string message, Exception innerException)
        : base(message, innerException) { }
}
