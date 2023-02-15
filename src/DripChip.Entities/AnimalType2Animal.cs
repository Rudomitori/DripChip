namespace DripChip.Entities;

public sealed class AnimalType2Animal
{
    public long AnimalTypeId { get; set; }
    public AnimalType? AnimalType { get; set; }

    public long AnimalId { get; set; }
    public Animal? Animal { get; set; }
}
