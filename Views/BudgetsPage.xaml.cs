using BudgetEtu.Models;
using BudgetEtu.ViewModels;

namespace BudgetEtu.Views;

public partial class BudgetsPage : ContentPage
{
    private readonly BudgetsViewModel _viewModel;

    public BudgetsPage(BudgetsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }

    private void OnCategoryTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject { BindingContext: BudgetCategory category })
            _viewModel.EditBudgetCommand.Execute(category);
    }
}
