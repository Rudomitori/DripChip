using Common.Domain.Exceptions;
using DripChip.Domain.Accounts;
using DripChip.WebApi.ApiModel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DripChip.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class AccountsController : ControllerBase
{
    #region Constructor and dependencies

    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #endregion

    [HttpGet("{accountId:int}")]
    public async Task<ApiAccount> GetById(int accountId)
    {
        var response = await _mediator.Send(
            new GetAccounts
            {
                Ids = new List<int> { accountId },
                Offset = 0,
                Size = 1
            }
        );

        if (!response.Accounts.Any())
            throw new NotFoundException($"Account with id \"{accountId}\" was not found");

        return response.Accounts.First();
    }

    [HttpGet("search")]
    public async Task<IEnumerable<ApiAccount>> Search([FromQuery] SearchRequestDto dto)
    {
        var response = await _mediator.Send(
            new GetAccounts
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Offset = dto.From,
                Size = dto.Size,
            }
        );

        return response.Accounts.Select(x => (ApiAccount)x);
    }

    public sealed class SearchRequestDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int From { get; set; } = 0;
        public int Size { get; set; } = 10;
    }

    [Authorize]
    [HttpPut("{accountId:int}")]
    public async Task<ApiAccount> UpdateAccount(
        int accountId,
        [FromBody] UpdateAccountRequestDto dto
    )
    {
        var response = await _mediator.Send(
            new UpdateAccount
            {
                Id = accountId,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Password = dto.Password
            }
        );

        return response.Account;
    }

    public sealed class UpdateAccountRequestDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    [Authorize]
    [HttpDelete("{accountId:int}")]
    public async Task DeleteAccount(int accountId)
    {
        await _mediator.Send(new DeleteAccount { Id = accountId });
    }
}
