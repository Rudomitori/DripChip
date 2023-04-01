// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using DripChip.Entities;

namespace DripChip.WebApi.ApiModel;

public sealed class ApiAnimal
{
    public long Id { get; set; }
    public List<long> AnimalTypes { get; set; }
    public float Weight { get; set; }
    public float Length { get; set; }
    public float Height { get; set; }
    public Gender Gender { get; set; }
    public LifeStatus LifeStatus { get; set; }
    public DateTime ChippingDateTime { get; set; }
    public int ChipperId { get; set; }
    public long ChippingLocationId { get; set; }
    public List<long> VisitedLocations { get; set; }
    public DateTime? DeathDateTime { get; set; }

    public static implicit operator ApiAnimal(Animal animal) =>
        new ApiAnimal
        {
            Id = animal.Id,
            AnimalTypes = animal.AnimalType2Animals.Select(x => x.AnimalTypeId).ToList(),
            Weight = animal.Weight,
            Length = animal.Length,
            Height = animal.Height,
            Gender = animal.Gender,
            LifeStatus = animal.LifeStatus,
            ChipperId = animal.ChipperId,
            ChippingDateTime = animal.ChippingDateTime,
            ChippingLocationId = animal.ChippingLocationId,
            VisitedLocations = animal.LocationVisits.Select(x => x.Id).ToList(),
            DeathDateTime = animal.DeathDateTime
        };
}
