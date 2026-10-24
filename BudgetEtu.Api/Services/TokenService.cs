using System.Security.Claims;
using System.Text;
using BudgetEtu.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BudgetEtu.Api.Services;

/// <summary>Fabrique le jeton JWT renvoyé à la connexion. L'application le renvoie ensuite dans « Authorization: Bearer ».</summary>
public class TokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiresAt) Create(User user)
    {
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim("name", user.FullName)
            }),
            Expires = expires,
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expires);
    }
}
