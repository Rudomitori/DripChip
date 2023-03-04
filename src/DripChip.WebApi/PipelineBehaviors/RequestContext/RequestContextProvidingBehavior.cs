using DripChip.Domain.Requests;
using MediatR;

namespace DripChip.WebApi.PipelineBehaviors.RequestContext;

public sealed class RequestContextProvidingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : RequestBase<TResponse>
{
    #region Constructor and dependencies

    private readonly IHttpContextAccessor _httpContextAccessor;

    public RequestContextProvidingBehavior(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    #endregion

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        request.Context = new WebApiRequestContext
        {
            UserId = _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated
                ? int.Parse(
                    _httpContextAccessor.HttpContext.User.Claims.First(x => x.Type is "id").Value
                )
                : null
        };

        return next();
    }
}
