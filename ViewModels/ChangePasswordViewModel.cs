using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetEtu.ViewModels;

/// <summary>Écran « Changer le mot de passe » (ouvert depuis le Profil).</summary>
public partial class ChangePasswordViewModel : FormViewModelBase
{
    private readonly IAuthService _auth;

    public ChangePasswordViewModel(IAuthService auth)
    {
        _auth = auth;
    }

    [ObservableProperty] private string currentPassword = string.Empty;
    [ObservableProperty] private string newPassword = string.Empty;
    [ObservableProperty] private string confirmPassword = string.Empty;

    [ObservableProperty] private string currentPasswordError = string.Empty;
    [ObservableProperty] private string newPasswordError = string.Empty;
    [ObservableProperty] private string confirmPasswordError = string.Empty;

    public void Reset()
    {
        CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
        CurrentPasswordError = NewPasswordError = ConfirmPasswordError = BannerMessage = string.Empty;
    }

    [RelayCommand]
    private Task Close() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task Save()
    {
        CurrentPasswordError = NewPasswordError = ConfirmPasswordError = string.Empty;

        if (string.IsNullOrEmpty(CurrentPassword))
            CurrentPasswordError = "Saisis ton mot de passe actuel.";

        if (string.IsNullOrEmpty(NewPassword))
            NewPasswordError = "Choisis un nouveau mot de passe.";
        else if (NewPassword.Length < 6)
            NewPasswordError = "Le mot de passe doit contenir au moins 6 caractères.";
        else if (NewPassword == CurrentPassword)
            NewPasswordError = "Le nouveau mot de passe doit être différent de l'ancien.";

        if (string.IsNullOrEmpty(ConfirmPassword))
            ConfirmPasswordError = "Confirme ton nouveau mot de passe.";
        else if (ConfirmPassword != NewPassword)
            ConfirmPasswordError = "Les mots de passe ne correspondent pas.";

        if (CurrentPasswordError.Length + NewPasswordError.Length + ConfirmPasswordError.Length > 0)
            return;

        await SubmitAsync(async () =>
        {
            await _auth.ChangePasswordAsync(CurrentPassword, NewPassword);

            await Shell.Current.DisplayAlert(
                "Mot de passe modifié", "Ton mot de passe a bien été mis à jour.", "OK");

            await Shell.Current.GoToAsync("..");
        });
    }
}
