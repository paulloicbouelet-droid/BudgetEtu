using BudgetEtu.Api.Data;
using BudgetEtu.Api.Models;
using BudgetEtu.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetEtu.Api.Controllers;

/// <summary>Profil de l'utilisateur connecté (écran « Profil »).</summary>
[ApiController]
[Route("api/me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _hasher;

    public MeController(AppDbContext db, IPasswordHasher<User> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> Get()
    {
        var userId = User.GetUserId();
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

        return user is null
            ? Unauthorized()
            : Ok(new UserDto(user.Id, user.FullName, user.Email));
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userId = User.GetUserId();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
            return Unauthorized();

        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword ?? string.Empty)
            == PasswordVerificationResult.Failed)
            return Problem("Le mot de passe actuel est incorrect.", statusCode: StatusCodes.Status400BadRequest);

        if ((request.NewPassword ?? string.Empty).Length < 6)
            return Problem("Le nouveau mot de passe doit contenir au moins 6 caractères.", statusCode: StatusCodes.Status400BadRequest);

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword!);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
