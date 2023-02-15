﻿// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using DripChip.Entities;

namespace DripChip.WebApi.ApiModel;

public sealed class ApiAnimalType
{
    public required long Id { get; set; }
    public required string Type { get; set; }

    public static implicit operator ApiAnimalType(AnimalType animalType) =>
        new ApiAnimalType { Id = animalType.Id, Type = animalType.Type };
}
