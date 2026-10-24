using BudgetEtu.Models;

namespace BudgetEtu.Services;

public interface IAuthService
{
    UserProfile? CurrentUser { get; }
    Task LoginAsync(string email, string password);
    Task RegisterAsync(string fullName, string email, string password);
    Task SendResetLinkAsync(string email);
    Task ChangePasswordAsync(string currentPassword, string newPassword);
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

    public async Task ChangePasswordAsync(string currentPassword, string newPassword)
    {
        await Simulation.EnterAsync();

        var index = _users.FindIndex(u =>
            string.Equals(u.Email, CurrentUser?.Email, StringComparison.OrdinalIgnoreCase));

        if (index < 0)
            throw new AppException(ErrorKind.SessionExpired, "Ta session a expiré.");

        if (_users[index].Password != currentPassword)
            throw new AppException(ErrorKind.Validation, "Le mot de passe actuel est incorrect.");

        if (newPassword.Length < 6)
            throw new AppException(ErrorKind.Validation, "Le nouveau mot de passe doit contenir au moins 6 caractères.");

        var user = _users[index];
        _users[index] = (user.FullName, user.Email, newPassword);
    }

    public void Logout() => CurrentUser = null;
}
