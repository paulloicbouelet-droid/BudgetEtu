using BudgetEtu.ViewModels;

namespace BudgetEtu.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Reset(); // efface le mot de passe et les erreurs (ex. après une déconnexion)
    }
}
