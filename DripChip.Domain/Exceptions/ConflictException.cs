﻿namespace DripChip.Domain.Exceptions;

public class ConflictException : DomainExceptionBase
{
    public ConflictException(string message)
        : base(message) { }
}
