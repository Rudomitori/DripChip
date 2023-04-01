using Common.Domain.Exceptions;
using DripChip.Domain.AnimalTypes;
using DripChip.WebApi.ApiModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DripChip.WebApi.Controllers;

[ApiController]
[Route("animals/types")]
public class AnimalTypesController : ControllerBase
{
    #region Constructor and dependencies

    private readonly IMediator _mediator;

    public AnimalTypesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #endregion

    [HttpGet("{typeId:long}")]
    public async Task<ApiAnimalType> GetAnimalTypeById(long typeId)
    {
        var response = await _mediator.Send(new GetAnimalTypes { Ids = new List<long> { typeId } });

        if (!response.AnimalTypes.Any())
            throw new NotFoundException($"Animal type with id {typeId} was not found");

        return response.AnimalTypes.First();
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiAnimalType>> CreateAnimalType(CreateAnimalTypeRequestDto dto)
    {
        var response = await _mediator.Send(new CreateAnimalType { Type = dto.Type });

        return Created(
            $"/animals/types/{response.AnimalType.Id}",
            (ApiAnimalType)response.AnimalType
        );
    }

    public sealed class CreateAnimalTypeRequestDto
    {
        public string Type { get; set; }
    }

    [Authorize]
    [HttpPut("{typeId:long}")]
    public async Task<ApiAnimalType> UpdateAnimalType(long typeId, UpdateAnimalTypeRequestDto dto)
    {
        var response = await _mediator.Send(new UpdateAnimalType { Id = typeId, Type = dto.Type });

        return response.AnimalType;
    }

    public sealed class UpdateAnimalTypeRequestDto
    {
        public string Type { get; set; }
    }

    [Authorize]
    [HttpDelete("{typeId:long}")]
    public async Task DeleteAnimalType(long typeId)
    {
        await _mediator.Send(new DeleteAnimalType { Id = typeId });
    }
}
