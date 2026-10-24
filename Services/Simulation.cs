namespace BudgetEtu.Services;

public enum DemoScenario
{
    Normal,
    Offline,
    ServerError,
    SessionExpired,
    Empty
}

/// <summary>
/// Menu de démonstration pour provoquer les écrans d'erreur (hors ligne, erreur serveur, session expirée,
/// aucune donnée) sans avoir à couper le serveur. Ouverture : menu « Démo » (Windows) ou 5 appuis sur un titre.
/// En fonctionnement normal, ce mécanisme ne fait rien : les vraies erreurs viennent de <see cref="ApiClient"/>.
/// </summary>
public static class Simulation
{
    public static DemoScenario Scenario { get; set; } = DemoScenario.Normal;

    /// <summary>À appeler au début de chaque appel de service.</summary>
    public static async Task EnterAsync(bool requiresSession = true)
    {
        // Fonctionnement normal : rien à simuler (avec le vrai serveur, la vraie latence suffit).
        if (Scenario == DemoScenario.Normal)
            return;

        await Task.Delay(600); // latence simulée, pour voir l'état « chargement » avant l'erreur

        switch (Scenario)
        {
            case DemoScenario.Offline:
                throw new AppException(ErrorKind.Offline, "Pas de connexion internet.");
            case DemoScenario.ServerError:
                throw new AppException(ErrorKind.Server, "Le serveur ne répond pas.");
            case DemoScenario.SessionExpired when requiresSession:
                throw new AppException(ErrorKind.SessionExpired, "Ta session a expiré.");
        }
    }
}
