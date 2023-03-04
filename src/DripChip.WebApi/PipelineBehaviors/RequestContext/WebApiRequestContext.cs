using DripChip.Domain.Requests;

namespace DripChip.WebApi.PipelineBehaviors.RequestContext;

public sealed class WebApiRequestContext : IRequestContext
{
    public required int? UserId { get; init; }
}
