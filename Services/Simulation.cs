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
/// Simule les conditions réseau/serveur tant qu'il n'y a pas de vrai backend.
/// Menu caché pour tester tous les écrans d'erreur : appuyer 5 fois sur le titre « Mon Profil ».
///
/// Quand tu brancheras une vraie API, remplace <see cref="EnterAsync"/> par un vrai test
/// (Connectivity.Current.NetworkAccess, codes HTTP…) qui lève les mêmes <see cref="AppException"/>.
/// </summary>
public static class Simulation
{
    public static DemoScenario Scenario { get; set; } = DemoScenario.Normal;

    /// <summary>À appeler au début de chaque appel de service.</summary>
    public static async Task EnterAsync(bool requiresSession = true)
    {
        await Task.Delay(600); // latence réseau simulée, pour voir l'état « chargement »

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
