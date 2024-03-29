﻿using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace DripChip.Entities;

public class Account : IdentityUser<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public Role Role { get; set; }

    private string _email;
    public override string Email
    {
        get => _email;
        set => _userName = _email = value;
    }

    private string _userName; // For EF Core
    public override string UserName
    {
        get => _email;
        set => _userName = _email = value;
    }

    public List<Claim> GetClaims() =>
        new List<Claim>
        {
            new Claim("id", Id.ToString()),
            new Claim("email", Email),
            new Claim("firstName", FirstName),
            new Claim("lastName", LastName),
            new Claim("role", Role.ToString())
        };
}
