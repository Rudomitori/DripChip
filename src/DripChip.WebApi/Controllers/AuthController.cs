// ReSharper disable UnusedAutoPropertyAccessor.Global

using DripChip.Domain.Accounts;
using DripChip.WebApi.ApiModel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DripChip.WebApi.Controllers;

[ApiController]
public sealed class AuthController : ControllerBase
{
    #region Constructor and dependencies

    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #endregion

    [HttpPost("registration")]
    public async Task<ActionResult<ApiAccount>> Register(
        [FromBody] [BindRequired] RegisterRequestDto dto
    )
    {
        if (HttpContext.User.Identity is { IsAuthenticated: true })
            return Forbid();

        var response = await _mediator.Send(
            new CreateAccount
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = dto.Password
            }
        );

        return Created($"/account/{response.Account.Id}", (ApiAccount)response.Account);
    }

    public sealed class RegisterRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
