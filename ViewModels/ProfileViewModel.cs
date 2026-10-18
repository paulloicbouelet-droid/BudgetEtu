using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetEtu.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IAuthService _auth;

    public ProfileViewModel(IAuthService auth)
    {
        _auth = auth;
        Refresh();
    }

    public IList<string> Currencies { get; } = new[] { "FCFA", "EUR", "USD" };
    public IList<string> Periods { get; } = new[] { "Hebdomadaire", "Mensuelle", "Annuelle" };

    [ObservableProperty] private string initials = "?";
    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string email = string.Empty;

    [ObservableProperty] private string selectedCurrency = "FCFA";
    [ObservableProperty] private string selectedPeriod = "Mensuelle";
    [ObservableProperty] private bool pushNotificationsEnabled = true;

    /// <summary>Recharge les infos de l'utilisateur connecté (appelé à chaque affichage de l'écran).</summary>
    public void Refresh()
    {
        var user = _auth.CurrentUser;
        Initials = user?.Initials ?? "?";
        FullName = user?.FullName ?? string.Empty;
        Email = user?.Email ?? string.Empty;
    }

    [RelayCommand]
    private Task ChangePassword()
        => Shell.Current.DisplayAlert(
            "Changer le mot de passe",
            "Cette fonctionnalité sera disponible lorsque l'application sera reliée au serveur.",
            "OK");

    [RelayCommand]
    private async Task Logout()
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Déconnexion", "Veux-tu vraiment te déconnecter ?", "Se déconnecter", "Annuler");

        if (!confirmed)
            return;

        _auth.Logout();
        await Shell.Current.GoToAsync("//Login");
    }

    /// <summary>Menu caché de démonstration (5 appuis sur le titre) : voir SimulationMenu.</summary>
    [RelayCommand]
    private Task ShowSimulationMenu() => SimulationMenu.ShowAsync();
}
