using NetTopologySuite.Geometries;

namespace DripChip.Entities;

public sealed class Location
{
    public long Id { get; set; }

    public required Point Coordinates { get; set; }
}
