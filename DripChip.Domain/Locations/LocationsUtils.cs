using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace DripChip.Domain.Locations;

public static class LocationsUtils
{
    // Source: https://stackoverflow.com/questions/365826/calculate-distance-between-2-gps-coordinates
    public static double GetDistanceBetweenLongLatInMeters(
        double lon1,
        double lat1,
        double lon2,
        double lat2
    )
    {
        const double earthRadiusInM = 6371 * 1000;

        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        lat1 = lat1 * Math.PI / 180;
        lat2 = lat2 * Math.PI / 180;

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Sin(dLon) * Math.Sin(dLon) * Math.Cos(lat1) * Math.Cos(lat2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusInM * c;
    }

    public static readonly NtsGeometryServices LonLatGeometryServices = new NtsGeometryServices(
        CoordinateArraySequenceFactory.Instance,
        new PrecisionModel(1000d),
        4326, // longitude and latitude
        GeometryOverlay.NG,
        new CoordinateEqualityComparer()
    );
}
