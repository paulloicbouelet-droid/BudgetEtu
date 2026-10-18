using BudgetEtu.ViewModels;

namespace BudgetEtu.Views;

public partial class TransactionPage : ContentPage
{
    public TransactionPage(TransactionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
