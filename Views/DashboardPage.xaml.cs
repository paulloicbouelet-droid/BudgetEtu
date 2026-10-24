using BudgetEtu.Models;
using BudgetEtu.ViewModels;

namespace BudgetEtu.Views;

public partial class DashboardPage : ContentPage
{
    private readonly DashboardViewModel _viewModel;

    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }

    private void OnDeleteTransactionTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: Transaction transaction })
            _viewModel.DeleteTransactionCommand.Execute(transaction);
    }
}
