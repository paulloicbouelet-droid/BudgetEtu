using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetEtu.ViewModels;

public partial class LoginViewModel : FormViewModelBase
{
    private readonly IAuthService _auth;

    public LoginViewModel(IAuthService auth)
    {
        _auth = auth;
    }

    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string emailError = string.Empty;
    [ObservableProperty] private string passwordError = string.Empty;

    public void Reset()
    {
        Password = string.Empty;
        EmailError = PasswordError = BannerMessage = string.Empty;
    }

    [RelayCommand]
    private async Task Login()
    {
        EmailError = PasswordError = string.Empty;

        if (string.IsNullOrWhiteSpace(Email))
            EmailError = "Saisis ton adresse e-mail.";
        else if (!IsValidEmail(Email))
            EmailError = "Adresse e-mail invalide (ex. etudiant@univ.cm).";

        if (string.IsNullOrEmpty(Password))
            PasswordError = "Saisis ton mot de passe.";

        if (EmailError.Length > 0 || PasswordError.Length > 0)
            return;

        await SubmitAsync(async () =>
        {
            await _auth.LoginAsync(Email, Password);
            await Shell.Current.GoToAsync("//Dashboard");
        });
    }

    [RelayCommand]
    private Task GoToForgotPassword() => Shell.Current.GoToAsync("//ForgotPassword");

    [RelayCommand]
    private Task GoToRegister() => Shell.Current.GoToAsync("//Register");

    /// <summary>5 appuis sur le titre « BudgetEtu » : menu de démo pour simuler hors ligne / erreur serveur.</summary>
    [RelayCommand]
    private Task ShowSimulationMenu() => SimulationMenu.ShowAsync();
}
