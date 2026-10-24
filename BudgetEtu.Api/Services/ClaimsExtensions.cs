using System.Security.Claims;

namespace BudgetEtu.Api.Services;

public static class ClaimsExtensions
{
    /// <summary>Id de l'utilisateur connecté, lu dans le jeton JWT (revendication « sub »).</summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var id) ? id : throw new UnauthorizedAccessException();
    }
}
