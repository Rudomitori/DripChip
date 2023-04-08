using DripChip.Entities;

namespace DripChip.WebApi.ApiModel;

public sealed record ApiArea(long Id, string Name, List<AreaPoint> AreaPoints)
{
    public static implicit operator ApiArea(Area area) =>
        new ApiArea(
            Id: area.Id,
            Name: area.Name,
            AreaPoints: area.Geometry.Coordinates
                .Select(x => new AreaPoint(Longitude: x.X, Latitude: x.Y))
                .ToList()
        );
}

public record struct AreaPoint(double Longitude, double Latitude);
