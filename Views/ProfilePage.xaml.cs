using BudgetEtu.ViewModels;

namespace BudgetEtu.Views;

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _viewModel;

    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Refresh(); // l'utilisateur a pu changer (déconnexion puis nouvelle connexion)
    }
}
