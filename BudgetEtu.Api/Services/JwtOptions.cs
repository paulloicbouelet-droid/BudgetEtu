namespace BudgetEtu.Api.Services;

/// <summary>Réglages du jeton JWT (section « Jwt » de appsettings.json).</summary>
public class JwtOptions
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "BudgetEtu.Api";
    public string Audience { get; set; } = "BudgetEtu.App";
    public int ExpiresMinutes { get; set; } = 120;
}
