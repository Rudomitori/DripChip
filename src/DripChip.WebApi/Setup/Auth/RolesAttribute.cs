using DripChip.Entities;
using Microsoft.AspNetCore.Authorization;

namespace DripChip.WebApi.Setup.Auth;

public class RolesAttribute : AuthorizeAttribute
{
    public RolesAttribute(params Role[] roles)
    {
        Roles = string.Join(", ", roles);
    }
}
