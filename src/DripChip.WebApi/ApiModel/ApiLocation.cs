// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using Location = DripChip.Entities.Location;

namespace DripChip.WebApi.ApiModel;

public sealed class ApiLocation
{
    public long Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public static implicit operator ApiLocation(Location location) =>
        new ApiLocation
        {
            Id = location.Id,
            Latitude = location.Coordinates.Y,
            Longitude = location.Coordinates.X
        };
}
