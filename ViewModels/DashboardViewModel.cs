using System.Collections.ObjectModel;
using BudgetEtu.Models;
using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetEtu.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IAuthService _auth;
    private readonly IBudgetService _budget;

    public DashboardViewModel(IAuthService auth, IBudgetService budget)
    {
        _auth = auth;
        _budget = budget;
    }

    [ObservableProperty] private string greeting = "Bonjour 👋";
    [ObservableProperty] private string balanceText = string.Empty;
    [ObservableProperty] private string incomeText = string.Empty;
    [ObservableProperty] private string expenseText = string.Empty;

    public ObservableCollection<Transaction> Recent { get; } = new();

    protected override string EmptyIcon => "🧾";
    protected override string EmptyTitle => "Aucune transaction";
    protected override string EmptyMessage => "Appuie sur + pour ajouter ta première dépense ou ton premier revenu.";
    protected override string EmptyActionText => "Ajouter une transaction";

    protected override Task OnEmptyActionAsync() => Shell.Current.GoToAsync("Transaction");

    /// <summary>5 appuis sur le message de bienvenue : menu de démo pour simuler les écrans d'erreur.</summary>
    [RelayCommand]
    private Task ShowSimulationMenu() => SimulationMenu.ShowAsync();

    protected override async Task<bool> FetchAsync()
    {
        var data = await _budget.GetDashboardAsync();

        var firstName = _auth.CurrentUser?.FirstName;
        Greeting = string.IsNullOrEmpty(firstName) ? "Bonjour 👋" : $"Bonjour, {firstName} 👋";

        BalanceText = Fcfa.Amount(data.Balance);
        IncomeText = $"+ {Fcfa.Amount(data.Income)}";
        ExpenseText = $"- {Fcfa.Amount(data.Expense)}";

        Recent.Clear();
        foreach (var transaction in data.Recent)
            Recent.Add(transaction);

        return data.Recent.Count > 0;
    }
}
