using MediatR;

namespace DripChip.Domain.Requests;

public abstract class RequestBase<TResponse> : IRequest<TResponse>
{
    public IRequestContext Context { get; set; }
}
