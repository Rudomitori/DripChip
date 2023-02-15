using Microsoft.AspNetCore.Mvc;

namespace DripChip.WebApi.Utils;

public static class ControllerExtensions
{
    /// <summary>
    /// Get the user id from <see cref="HttpContext"/>
    /// </summary>
    public static int? GetUserId(this ControllerBase controller)
    {
        return controller.HttpContext.User.FindFirst("id")?.Value is { } str
            ? int.Parse(str)
            : null;
    }
}
