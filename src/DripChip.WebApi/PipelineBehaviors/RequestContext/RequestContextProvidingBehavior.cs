using DripChip.Domain.Requests;
using DripChip.Entities;
using MediatR;
using static DripChip.Entities.Role;

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
        var roleIsParsed = Enum.TryParse(
            _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(x => x.Type is "role")
                ?.Value,
            out Role role
        );

        var idIsParsed = int.TryParse(
            _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(x => x.Type is "id")
                ?.Value,
            out var userId
        );

        request.Context = new WebApiRequestContext
        {
            UserId = idIsParsed ? userId : null,
            UserRole = roleIsParsed ? role : null
        };

        return next();
    }
}
