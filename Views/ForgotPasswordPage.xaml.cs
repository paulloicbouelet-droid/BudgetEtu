using BudgetEtu.ViewModels;

namespace BudgetEtu.Views;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly ForgotPasswordViewModel _viewModel;

    public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Reset();
    }
}
