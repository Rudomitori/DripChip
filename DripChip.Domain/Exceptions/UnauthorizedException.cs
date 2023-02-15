namespace DripChip.Domain.Exceptions;

public class UnauthorizedException : DomainExceptionBase
{
    public UnauthorizedException(string message)
        : base(message) { }
}
