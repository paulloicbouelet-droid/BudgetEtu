using BudgetEtu.ViewModels;

namespace BudgetEtu.Views;

public partial class BudgetFormPage : ContentPage
{
    public BudgetFormPage(BudgetFormViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
