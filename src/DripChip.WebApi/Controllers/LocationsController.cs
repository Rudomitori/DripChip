using DripChip.Domain.Exceptions;
using DripChip.Domain.Locations;
using DripChip.Domain.Locations.Requests;
using DripChip.WebApi.ApiModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace DripChip.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class LocationsController : ControllerBase
{
    #region Constructor and dependencies

    private readonly IMediator _mediator;
    private readonly NtsGeometryServices _geometryServices;

    public LocationsController(IMediator mediator, NtsGeometryServices geometryServices)
    {
        _mediator = mediator;
        _geometryServices = geometryServices;
    }

    #endregion

    [HttpGet("{pointId:long}")]
    public async Task<ApiLocation> GetLocationById(long pointId)
    {
        var response = await _mediator.Send(new GetLocations { Ids = new List<long> { pointId } });

        if (!response.Locations.Any())
            throw new NotFoundException($"Location with id {pointId} was not found");

        return response.Locations.First();
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiLocation>> CreateLocation(CreateLocationRequestDto dto)
    {
        var geometryFactory = _geometryServices.CreateGeometryFactory();
        var coordinates = geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude));
        var response = await _mediator.Send(new CreateLocation { Coordinates = coordinates });

        return Created($"/locations/{response.Location.Id}", (ApiLocation)response.Location);
    }

    public sealed class CreateLocationRequestDto
    {
        public required double Latitude { get; set; }
        public required double Longitude { get; set; }
    }

    [Authorize]
    [HttpPut("{pointId:long}")]
    public async Task<ApiLocation> UpdateLocation(long pointId, UpdateLocationRequestDto dto)
    {
        var geometryFactory = _geometryServices.CreateGeometryFactory();
        var coordinates = geometryFactory.CreatePoint(new Coordinate(dto.Longitude, dto.Latitude));
        var response = await _mediator.Send(
            new UpdateLocation { Id = pointId, Coordinates = coordinates }
        );

        return response.Location;
    }

    public sealed class UpdateLocationRequestDto
    {
        public required double Latitude { get; set; }
        public required double Longitude { get; set; }
    }

    [Authorize]
    [HttpDelete("{pointId:long}")]
    public async Task DeleteLocation(long pointId)
    {
        await _mediator.Send(new DeleteLocation { Id = pointId });
    }
}
