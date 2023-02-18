// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using DripChip.Entities;

namespace DripChip.WebApi.ApiModel;

public sealed class ApiAccount
{
    public required int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }

    public static implicit operator ApiAccount(Account account) =>
        new()
        {
            Id = account.Id,
            FirstName = account.FirstName!,
            LastName = account.LastName!,
            Email = account.Email!,
        };
}
