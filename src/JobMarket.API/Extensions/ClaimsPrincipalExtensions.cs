using System.Security.Claims;

namespace JobMarket.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        string? value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");

        if (Guid.TryParse(value, out Guid userId))
            return userId;

        throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
    }
}
