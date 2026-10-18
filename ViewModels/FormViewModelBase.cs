using System.Text.RegularExpressions;
using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetEtu.ViewModels;

/// <summary>
/// Base des écrans à formulaire (Connexion, Inscription, Mot de passe oublié, Saisie).
/// Fournit l'état « en cours d'envoi », le bandeau d'erreur global et les validateurs.
/// Les erreurs de champ suivent le modèle de la maquette « 4_SaisieTransaction_Erreur ».
/// </summary>
public abstract partial class FormViewModelBase : ObservableObject
{
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool isBusy;

    /// <summary>Erreur globale (hors ligne, serveur, identifiants incorrects…) affichée en bandeau rouge.</summary>
    [ObservableProperty]
    private string bannerMessage = string.Empty;

    public bool IsNotBusy => !IsBusy;

    protected static bool IsValidEmail(string? value)
        => !string.IsNullOrWhiteSpace(value) && EmailRegex.IsMatch(value.Trim());

    /// <summary>Exécute l'envoi du formulaire en gérant le bandeau d'erreur et l'état « occupé ».</summary>
    protected async Task SubmitAsync(Func<Task> action)
    {
        BannerMessage = string.Empty;
        IsBusy = true;

        try
        {
            await action();
        }
        catch (AppException ex)
        {
            BannerMessage = Describe(ex);
        }
        catch (Exception)
        {
            BannerMessage = "Une erreur inattendue est survenue. Réessaie dans un instant.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string Describe(AppException ex) => ex.Kind switch
    {
        ErrorKind.Offline => "Pas de connexion internet. Vérifie ton réseau puis réessaie.",
        ErrorKind.Server => "Le serveur ne répond pas pour le moment. Réessaie dans quelques instants.",
        ErrorKind.SessionExpired => "Ta session a expiré. Reconnecte-toi pour continuer.",
        _ => ex.Message
    };
}
