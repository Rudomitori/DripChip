using DripChip.Domain.Areas;
using DripChip.WebApi.ApiModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DripChip.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public sealed class AreasController : ControllerBase
{
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiArea>> GetArea(long id, [FromServices] IMediator mediator)
    {
        var response = await mediator.Send(new GetAreas { Ids = new List<long> { id } });

        if (response.Areas.Count == 0)
            return NotFound();

        return Ok((ApiArea)response.Areas[0]);
    }
}
