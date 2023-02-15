namespace DripChip.Entities;

public sealed class Animal
{
    public long Id { get; set; }
    public required float Weight { get; set; }
    public required float Length { get; set; }
    public required float Height { get; set; }
    public required Gender Gender { get; set; }
    public required LifeStatus LifeStatus { get; set; }
    public DateTime? DeathDateTime { get; set; }
    public required DateTime ChippingDateTime { get; set; }

    public int ChipperId { get; set; }
    public Account? Chipper { get; set; }

    public long ChippingLocationId { get; set; }
    public Location? ChippingLocation { get; set; }

    public List<AnimalType2Animal>? AnimalType2Animals { get; set; }
    public List<LocationVisit>? LocationVisits { get; set; }
}
