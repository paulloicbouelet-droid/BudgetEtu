namespace BudgetEtu.Services;

public enum ErrorKind
{
    /// <summary>Pas de connexion internet.</summary>
    Offline,

    /// <summary>Le serveur ne répond pas / erreur 5xx.</summary>
    Server,

    /// <summary>Session expirée (401).</summary>
    SessionExpired,

    /// <summary>Refus métier (identifiants incorrects, e-mail déjà utilisé…). Le message est affichable tel quel.</summary>
    Validation
}

/// <summary>
/// Erreur « attendue » levée par les services. Les ViewModels la transforment en écran d'erreur
/// (pages de données) ou en bandeau d'erreur (formulaires).
/// </summary>
public class AppException : Exception
{
    public ErrorKind Kind { get; }

    public AppException(ErrorKind kind, string message) : base(message)
    {
        Kind = kind;
    }
}
