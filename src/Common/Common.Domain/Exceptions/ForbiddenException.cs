﻿namespace Common.Domain.Exceptions;

public sealed class ForbiddenException : DomainExceptionBase
{
    public ForbiddenException(string message)
        : base(message) { }
}
