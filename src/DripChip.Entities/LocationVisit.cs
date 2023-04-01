namespace DripChip.Entities;

public sealed class LocationVisit
{
    public long Id { get; set; }

    public long AnimalId { get; set; }
    public Animal? Animal { get; set; }

    public long LocationId { get; set; }
    public Location? Location { get; set; }

    public DateTime VisitedAt { get; set; }
}
