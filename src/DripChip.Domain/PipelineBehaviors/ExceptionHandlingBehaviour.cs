using Common.Domain.Exceptions;
using FluentValidation;
using MediatR;

namespace DripChip.Domain.PipelineBehaviors;

public class ExceptionHandlingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return next();
        }
        catch (Exception e)
        {
            if (e is DomainExceptionBase or ValidationException)
                throw;

            throw new InternalException("Unexpected exception occurred", e);
        }
    }
}
