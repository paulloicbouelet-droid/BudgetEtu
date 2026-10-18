using BudgetEtu.ViewModels;

namespace BudgetEtu.Views;

public partial class AlertsPage : ContentPage
{
    private readonly AlertsViewModel _viewModel;

    public AlertsPage(AlertsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }
}
