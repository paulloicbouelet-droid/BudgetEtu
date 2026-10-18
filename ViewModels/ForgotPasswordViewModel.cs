using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetEtu.ViewModels;

public partial class ForgotPasswordViewModel : FormViewModelBase
{
    private readonly IAuthService _auth;

    public ForgotPasswordViewModel(IAuthService auth)
    {
        _auth = auth;
    }

    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string emailError = string.Empty;

    /// <summary>Confirmation affichée en bandeau vert une fois le lien envoyé.</summary>
    [ObservableProperty] private string successMessage = string.Empty;

    public void Reset()
    {
        Email = EmailError = BannerMessage = SuccessMessage = string.Empty;
    }

    [RelayCommand]
    private async Task SendLink()
    {
        EmailError = SuccessMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Email))
            EmailError = "Saisis ton adresse e-mail.";
        else if (!IsValidEmail(Email))
            EmailError = "Adresse e-mail invalide (ex. etudiant@univ.cm).";

        if (EmailError.Length > 0)
            return;

        await SubmitAsync(async () =>
        {
            await _auth.SendResetLinkAsync(Email);
            SuccessMessage = $"Lien envoyé à {Email.Trim()}. Pense à vérifier tes courriers indésirables.";
        });
    }

    [RelayCommand]
    private Task GoToLogin() => Shell.Current.GoToAsync("//Login");
}
