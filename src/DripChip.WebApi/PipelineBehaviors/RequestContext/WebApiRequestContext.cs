using DripChip.Domain.Requests;
using DripChip.Entities;

namespace DripChip.WebApi.PipelineBehaviors.RequestContext;

public sealed class WebApiRequestContext : IRequestContext
{
    public int? UserId { get; init; }
    public Role? UserRole { get; set; }
}
