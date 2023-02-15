using DripChip.Domain.Exceptions;
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
        catch (DomainExceptionBase e)
        {
            throw;
        }
        catch (ValidationException e)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new InternalException("Unexpected exception occurred", e);
        }
    }
}
