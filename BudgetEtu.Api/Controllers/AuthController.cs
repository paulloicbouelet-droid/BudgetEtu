using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using BudgetEtu.Api.Data;
using BudgetEtu.Api.Models;
using BudgetEtu.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetEtu.Api.Controllers;

/// <summary>Inscription, connexion, mot de passe oublié. Ces routes sont publiques (pas de jeton requis).</summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly AppDbContext _db;
    private readonly TokenService _tokens;
    private readonly IPasswordHasher<User> _hasher;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        AppDbContext db,
        TokenService tokens,
        IPasswordHasher<User> hasher,
        ILogger<AuthController> logger)
    {
        _db = db;
        _tokens = tokens;
        _hasher = hasher;
        _logger = logger;
    }

    /// <summary>Crée un compte (avec les enveloppes de budget par défaut) et connecte directement l'utilisateur.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var fullName = (request.FullName ?? string.Empty).Trim();
        var email = NormalizeEmail(request.Email);
        var password = request.Password ?? string.Empty;

        if (fullName.Length < 2)
            return Problem("Ton nom est trop court.", statusCode: StatusCodes.Status400BadRequest);
        if (!EmailRegex.IsMatch(email))
            return Problem("Adresse e-mail invalide.", statusCode: StatusCodes.Status400BadRequest);
        if (password.Length < 6)
            return Problem("Le mot de passe doit contenir au moins 6 caractères.", statusCode: StatusCodes.Status400BadRequest);

        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Problem("Un compte existe déjà avec cette adresse e-mail.", statusCode: StatusCodes.Status409Conflict);

        var user = new User { FullName = fullName, Email = email };
        user.PasswordHash = _hasher.HashPassword(user, password);
        user.Envelopes.AddRange(DbSeeder.DefaultEnvelopes());

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(BuildResponse(user));
    }

    /// <summary>Vérifie l'e-mail et le mot de passe, renvoie un jeton JWT.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        var valid = user is not null
            && _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password ?? string.Empty)
                != PasswordVerificationResult.Failed;

        // Même message dans les deux cas : on ne révèle pas si l'e-mail existe.
        if (!valid || user is null)
            return Problem("E-mail ou mot de passe incorrect.", statusCode: StatusCodes.Status401Unauthorized);

        return Ok(BuildResponse(user));
    }

    /// <summary>
    /// « Mot de passe oublié » : répond toujours 204 (on ne révèle pas si le compte existe).
    /// Sans serveur d'e-mails, le jeton est écrit dans la console du serveur ; il serait envoyé par e-mail en production.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is not null)
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(24));
            user.ResetTokenHash = HashToken(token);
            user.ResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(30);
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "[E-MAIL SIMULÉ] Jeton de réinitialisation pour {Email} (valable 30 min) : {Token}", user.Email, token);
        }

        return NoContent();
    }

    /// <summary>Choisit un nouveau mot de passe avec le jeton reçu par e-mail.</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        var tokenOk = user is not null
            && user.ResetTokenHash is not null
            && user.ResetTokenExpiresAt > DateTime.UtcNow
            && user.ResetTokenHash == HashToken(request.Token ?? string.Empty);

        if (!tokenOk || user is null)
            return Problem("Lien de réinitialisation invalide ou expiré.", statusCode: StatusCodes.Status400BadRequest);

        if ((request.NewPassword ?? string.Empty).Length < 6)
            return Problem("Le mot de passe doit contenir au moins 6 caractères.", statusCode: StatusCodes.Status400BadRequest);

        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword!);
        user.ResetTokenHash = null;
        user.ResetTokenExpiresAt = null;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = _tokens.Create(user);
        return new AuthResponse(token, expiresAt, new UserDto(user.Id, user.FullName, user.Email));
    }

    private static string NormalizeEmail(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
