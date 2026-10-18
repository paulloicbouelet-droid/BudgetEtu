using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetEtu.ViewModels;

public partial class RegisterViewModel : FormViewModelBase
{
    private readonly IAuthService _auth;

    public RegisterViewModel(IAuthService auth)
    {
        _auth = auth;
    }

    [ObservableProperty] private string fullName = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;

    [ObservableProperty] private string fullNameError = string.Empty;
    [ObservableProperty] private string emailError = string.Empty;
    [ObservableProperty] private string passwordError = string.Empty;
    [ObservableProperty] private string confirmPasswordError = string.Empty;

    public void Reset()
    {
        FullName = Email = Password = ConfirmPassword = string.Empty;
        FullNameError = EmailError = PasswordError = ConfirmPasswordError = BannerMessage = string.Empty;
    }

    [RelayCommand]
    private async Task Register()
    {
        FullNameError = EmailError = PasswordError = ConfirmPasswordError = string.Empty;

        if (string.IsNullOrWhiteSpace(FullName))
            FullNameError = "Saisis ton nom complet.";
        else if (FullName.Trim().Length < 2)
            FullNameError = "Ton nom est trop court.";

        if (string.IsNullOrWhiteSpace(Email))
            EmailError = "Saisis ton adresse e-mail.";
        else if (!IsValidEmail(Email))
            EmailError = "Adresse e-mail invalide (ex. etudiant@univ.cm).";

        if (string.IsNullOrEmpty(Password))
            PasswordError = "Choisis un mot de passe.";
        else if (Password.Length < 6)
            PasswordError = "Le mot de passe doit contenir au moins 6 caractères.";

        if (string.IsNullOrEmpty(ConfirmPassword))
            ConfirmPasswordError = "Confirme ton mot de passe.";
        else if (ConfirmPassword != Password)
            ConfirmPasswordError = "Les mots de passe ne correspondent pas.";

        if (FullNameError.Length + EmailError.Length + PasswordError.Length + ConfirmPasswordError.Length > 0)
            return;

        await SubmitAsync(async () =>
        {
            await _auth.RegisterAsync(FullName, Email, Password);
            await Shell.Current.GoToAsync("//Dashboard");
        });
    }

    [RelayCommand]
    private Task GoToLogin() => Shell.Current.GoToAsync("//Login");
}
