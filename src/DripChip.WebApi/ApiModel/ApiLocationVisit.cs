// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using DripChip.Entities;

namespace DripChip.WebApi.ApiModel;

public sealed class ApiLocationVisit
{
    public long Id { get; set; }
    public DateTime DateTimeOfVisitLocationPoint { get; set; }
    public long LocationPointId { get; set; }

    public static implicit operator ApiLocationVisit(LocationVisit visit) =>
        new ApiLocationVisit
        {
            Id = visit.Id,
            DateTimeOfVisitLocationPoint = visit.VisitedAt,
            LocationPointId = visit.LocationId
        };
}
