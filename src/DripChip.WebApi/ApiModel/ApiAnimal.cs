// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using DripChip.Entities;

namespace DripChip.WebApi.ApiModel;

public sealed class ApiAnimal
{
    public required long Id { get; set; }
    public required List<long> AnimalTypes { get; set; }
    public required float Weight { get; set; }
    public required float Length { get; set; }
    public required float Height { get; set; }
    public required Gender Gender { get; set; }
    public required LifeStatus LifeStatus { get; set; }
    public required DateTime ChippingDateTime { get; set; }
    public required int ChipperId { get; set; }
    public required long ChippingLocationId { get; set; }
    public required List<long> VisitedLocations { get; set; }
    public required DateTime? DeathDateTime { get; set; }

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
