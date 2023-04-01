using Common.Domain.Exceptions;
using DripChip.Domain.Animals;
using DripChip.Domain.LocationVisits;
using DripChip.Entities;
using DripChip.WebApi.ApiModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DripChip.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AnimalsController : ControllerBase
{
    #region Constructor and dependencies

    private readonly IMediator _mediator;

    public AnimalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #endregion

    [HttpGet("{animalId:long}")]
    public async Task<ApiAnimal> GetAnimalById(long animalId)
    {
        var response = await _mediator.Send(
            new GetAnimals
            {
                Ids = new List<long> { animalId },
                Offset = 0,
                Size = 1
            }
        );

        if (!response.Animals.Any())
            throw new NotFoundException($"Animal with id {animalId} was not found");

        return response.Animals.First();
    }

    [HttpGet("search")]
    public async Task<IEnumerable<ApiAnimal>> Search([FromQuery] SearchRequestDto dto)
    {
        var response = await _mediator.Send(
            new GetAnimals
            {
                MinChippingDateTime = dto.StartDateTime,
                MaxChippingDateTime = dto.EndDateTime,
                ChipperId = dto.ChipperId,
                ChippingLocationId = dto.ChippingLocationId,
                LifeStatus = dto.LifeStatus,
                Gender = dto.Gender,
                Offset = dto.From,
                Size = dto.Size
            }
        );

        return response.Animals.Select(x => (ApiAnimal)x);
    }

    public sealed class SearchRequestDto
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int? ChipperId { get; set; }
        public long? ChippingLocationId { get; set; }
        public LifeStatus? LifeStatus { get; set; }
        public Gender? Gender { get; set; }
        public int From { get; set; } = 0;
        public int Size { get; set; } = 10;
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiAnimal>> CreateAnimal(CreateAnimalRequestDto dto)
    {
        var response = await _mediator.Send(
            new CreateAnimal
            {
                AnimalTypeIds = dto.AnimalTypes,
                ChipperId = dto.ChipperId,
                ChippingLocationId = dto.ChippingLocationId,
                Gender = dto.Gender,
                Height = dto.Height,
                Length = dto.Length,
                Weight = dto.Weight
            }
        );

        return Created($"/animals/{response.Animal.Id}", (ApiAnimal)response.Animal);
    }

    public sealed class CreateAnimalRequestDto
    {
        public List<long> AnimalTypes { get; set; }
        public float Weight { get; set; }
        public float Height { get; set; }
        public float Length { get; set; }
        public Gender Gender { get; set; }
        public int ChipperId { get; set; }
        public int ChippingLocationId { get; set; }
    }

    [Authorize]
    [HttpPut("{animalId:long}")]
    public async Task<ApiAnimal> UpdateAnimal(long animalId, UpdateAnimalRequestDto dto)
    {
        var response = await _mediator.Send(
            new UpdateAnimal
            {
                Id = animalId,
                Weight = dto.Weight,
                Length = dto.Length,
                Height = dto.Height,
                Gender = dto.Gender,
                LifeStatus = dto.LifeStatus,
                ChipperId = dto.ChipperId,
                ChippingLocationId = dto.ChippingLocationId
            }
        );

        return response.Animal;
    }

    public sealed class UpdateAnimalRequestDto
    {
        public float Weight { get; set; }
        public float Length { get; set; }
        public float Height { get; set; }
        public Gender Gender { get; set; }
        public LifeStatus LifeStatus { get; set; }
        public int ChipperId { get; set; }
        public long ChippingLocationId { get; set; }
    }

    [Authorize]
    [HttpDelete("{animalId:long}")]
    public async Task DeleteAnimal(long animalId)
    {
        await _mediator.Send(new DeleteAnimal { Id = animalId });
    }

    [Authorize]
    [HttpPost("{animalId:long}/types/{typeid:long}")]
    public async Task<ActionResult<ApiAnimal>> AddTypeToAnimal(long animalId, long typeId)
    {
        var response = await _mediator.Send(
            new UpdateAnimal
            {
                Id = animalId,
                AnimalTypeIdsToAdd = new List<long> { typeId }
            }
        );

        return Created($"/animals/{animalId}/types/{typeId}", (ApiAnimal)response.Animal);
    }

    [Authorize]
    [HttpPut("{animalId:long}/types")]
    public async Task<ApiAnimal> ReplaceTypeForAnimal(
        long animalId,
        ReplaceTypeForAnimalRequestDto dto
    )
    {
        var response = await _mediator.Send(
            new UpdateAnimal
            {
                Id = animalId,
                AnimalTypeIdsToAdd = new List<long> { dto.NewTypeId },
                AnimalTypeIdsToRemove = new List<long> { dto.OldTypeId }
            }
        );

        return response.Animal;
    }

    public sealed class ReplaceTypeForAnimalRequestDto
    {
        public long OldTypeId { get; set; }
        public long NewTypeId { get; set; }
    }

    [Authorize]
    [HttpDelete("{animalId:long}/types/{typeId:long}")]
    public async Task<ApiAnimal> DeleteTypeForAnimal(long animalId, long typeId)
    {
        var response = await _mediator.Send(
            new UpdateAnimal
            {
                Id = animalId,
                AnimalTypeIdsToRemove = new List<long> { typeId }
            }
        );

        return response.Animal;
    }

    [HttpGet("{animalId:long}/locations")]
    public async Task<IEnumerable<ApiLocationVisit>> GetVisitedLocations(
        long animalId,
        [FromQuery] GetVisitedLocationsRequestDto dto
    )
    {
        var response = await _mediator.Send(
            new GetLocationVisits
            {
                VisitedByAnimalId = animalId,
                MinVisitedAt = dto.StartDateTime,
                MaxVisitedAt = dto.EndDateTime,
                Offset = dto.From,
                Size = dto.Size
            }
        );

        return response.LocationVisits.Select(x => (ApiLocationVisit)x);
    }

    public sealed class GetVisitedLocationsRequestDto
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int From { get; set; } = 0;
        public int Size { get; set; } = 10;
    }

    [Authorize]
    [HttpPost("{animalId:long}/locations/{pointId:long}")]
    public async Task<ActionResult<ApiLocationVisit>> AddVisitedLocationToAnimal(
        long animalId,
        long pointId
    )
    {
        var response = await _mediator.Send(
            new CreateLocationVisit { AnimalId = animalId, LocationId = pointId }
        );

        return Created(
            $"/animals/{animalId}/locations/{response.LocationVisit.Id}",
            (ApiLocationVisit)response.LocationVisit
        );
    }

    [Authorize]
    [HttpPut("{animalId:long}/locations")]
    public async Task<ApiLocationVisit> ReplaceVisitedLocation(
        long animalId,
        ReplaceVisitedLocationDto dto
    )
    {
        var response = await _mediator.Send(
            new UpdateLocationVisit
            {
                AnimalId = animalId,
                Id = dto.VisitedLocationPointId,
                LocationId = dto.LocationPointId
            }
        );

        return response.LocationVisit;
    }

    public sealed class ReplaceVisitedLocationDto
    {
        public long VisitedLocationPointId { get; set; }
        public long LocationPointId { get; set; }
    }

    [Authorize]
    [HttpDelete("{animalId:long}/locations/{visitedPointId:long}")]
    public async Task DeleteVisitedPoint(long animalId, long visitedPointId)
    {
        await _mediator.Send(new DeleteLocationVisit { AnimalId = animalId, Id = visitedPointId });
    }
}
