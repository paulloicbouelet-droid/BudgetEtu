using BudgetEtu.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace BudgetEtu.ViewModels;

/// <summary>
/// Menu caché de démonstration pour provoquer les écrans d'erreur sans backend :
/// hors ligne, erreur serveur, session expirée, aucune donnée.
/// Ouverture : 5 appuis rapides sur le titre de l'écran Connexion (« BudgetEtu ») ou d'Accueil (« Bonjour, … »).
/// </summary>
public static class SimulationMenu
{
    public static async Task ShowAsync()
    {
        const string normal = "Fonctionnement normal";
        const string offline = "Hors ligne";
        const string server = "Erreur serveur";
        const string session = "Session expirée";
        const string empty = "Aucune donnée";

        var choice = await Shell.Current.DisplayActionSheet(
            "Simuler un état (démo)", "Annuler", null, normal, offline, server, session, empty);

        var scenario = choice switch
        {
            offline => DemoScenario.Offline,
            server => DemoScenario.ServerError,
            session => DemoScenario.SessionExpired,
            empty => DemoScenario.Empty,
            normal => DemoScenario.Normal,
            _ => (DemoScenario?)null
        };

        if (scenario is null)
            return;

        Simulation.Scenario = scenario.Value;

        // Recharge les écrans déjà ouverts pour que le changement soit visible tout de suite.
        WeakReferenceMessenger.Default.Send(new DataChangedMessage());
    }
}
