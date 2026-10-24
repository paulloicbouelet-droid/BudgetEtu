using BudgetEtu.Models;

namespace BudgetEtu.Services;

/// <summary>
/// Authentification réelle : appelle /api/auth/* du serveur BudgetEtu.Api.
/// Compte de démonstration créé par le serveur : etudiant@univ.cm / budget123.
/// </summary>
public class ApiAuthService : IAuthService
{
    private readonly ApiClient _api;

    public ApiAuthService(ApiClient api)
    {
        _api = api;
    }

    public UserProfile? CurrentUser { get; private set; }

    public async Task LoginAsync(string email, string password)
    {
        var result = await _api.PostAsync<ApiAuthResponse>(
            "api/auth/login",
            new { email = email.Trim(), password },
            anonymous: true);

        StartSession(result);
    }

    public async Task RegisterAsync(string fullName, string email, string password)
    {
        var result = await _api.PostAsync<ApiAuthResponse>(
            "api/auth/register",
            new { fullName = fullName.Trim(), email = email.Trim(), password },
            anonymous: true);

        StartSession(result);
    }

    /// <summary>Le serveur répond toujours « OK », même si l'adresse n'a pas de compte (on ne révèle pas qui est inscrit).</summary>
    public Task SendResetLinkAsync(string email)
        => _api.PostAsync("api/auth/forgot-password", new { email = email.Trim() }, anonymous: true);

    public void Logout()
    {
        _api.Token = null;
        CurrentUser = null;
    }

    private void StartSession(ApiAuthResponse result)
    {
        _api.Token = result.Token;
        CurrentUser = new UserProfile(result.User.FullName, result.User.Email);

        // Une nouvelle connexion « renouvelle » la session dans le scénario de démo.
        if (Simulation.Scenario == DemoScenario.SessionExpired)
            Simulation.Scenario = DemoScenario.Normal;
    }
}
