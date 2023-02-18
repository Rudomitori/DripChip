// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using DripChip.Entities;

namespace DripChip.WebApi.ApiModel;

public sealed class ApiLocationVisit
{
    public required long Id { get; set; }
    public required DateTime DateTimeOfVisitLocationPoint { get; set; }
    public required long LocationPointId { get; set; }

    public static implicit operator ApiLocationVisit(LocationVisit visit) =>
        new ApiLocationVisit
        {
            Id = visit.Id,
            DateTimeOfVisitLocationPoint = visit.VisitedAt,
            LocationPointId = visit.LocationId
        };
}
