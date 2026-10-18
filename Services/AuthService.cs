using BudgetEtu.Models;

namespace BudgetEtu.Services;

public interface IAuthService
{
    UserProfile? CurrentUser { get; }
    Task LoginAsync(string email, string password);
    Task RegisterAsync(string fullName, string email, string password);
    Task SendResetLinkAsync(string email);
    void Logout();
}

/// <summary>
/// Implémentation fictive (en mémoire). Compte de démonstration :
///   e-mail : etudiant@univ.cm   mot de passe : budget123
/// À remplacer par un appel à ton backend.
/// </summary>
public class AuthService : IAuthService
{
    private readonly List<(string FullName, string Email, string Password)> _users = new()
    {
        ("Paul Loïc", "etudiant@univ.cm", "budget123")
    };

    public UserProfile? CurrentUser { get; private set; }

    public async Task LoginAsync(string email, string password)
    {
        await Simulation.EnterAsync(requiresSession: false);

        var user = _users.FirstOrDefault(u =>
            string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase) && u.Password == password);

        if (user.Email is null)
            throw new AppException(ErrorKind.Validation, "E-mail ou mot de passe incorrect.");

        // Une nouvelle connexion « renouvelle » la session dans le scénario de démo.
        if (Simulation.Scenario == DemoScenario.SessionExpired)
            Simulation.Scenario = DemoScenario.Normal;

        CurrentUser = new UserProfile(user.FullName, user.Email);
    }

    public async Task RegisterAsync(string fullName, string email, string password)
    {
        await Simulation.EnterAsync(requiresSession: false);

        if (_users.Any(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new AppException(ErrorKind.Validation, "Un compte existe déjà avec cette adresse e-mail.");

        _users.Add((fullName.Trim(), email.Trim(), password));
        CurrentUser = new UserProfile(fullName.Trim(), email.Trim());
    }

    public async Task SendResetLinkAsync(string email)
    {
        await Simulation.EnterAsync(requiresSession: false);

        if (!_users.Any(u => string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new AppException(ErrorKind.Validation, "Aucun compte n'est associé à cette adresse e-mail.");
    }

    public void Logout() => CurrentUser = null;
}
