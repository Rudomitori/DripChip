using NetTopologySuite.Geometries;

namespace DripChip.Entities;

public class Area
{
    public long Id { get; set; }
    public string Name { get; set; }
    public LinearRing Geometry { get; set; }
}
