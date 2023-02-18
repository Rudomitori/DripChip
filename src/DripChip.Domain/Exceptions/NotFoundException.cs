namespace DripChip.Domain.Exceptions;

public class NotFoundException : DomainExceptionBase
{
    public NotFoundException(string message)
        : base(message) { }
}
