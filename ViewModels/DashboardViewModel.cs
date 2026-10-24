using System.Collections.ObjectModel;
using BudgetEtu.Models;
using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

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

    /// <summary>Corbeille d'une transaction récente : demande confirmation, supprime, puis recharge les écrans.</summary>
    [RelayCommand]
    private async Task DeleteTransaction(Transaction transaction)
    {
        var confirmed = await Shell.Current.DisplayAlert(
            "Supprimer la transaction",
            $"Supprimer « {transaction.Category} » ({transaction.AmountText}) ?",
            "Supprimer", "Annuler");

        if (!confirmed)
            return;

        try
        {
            await _budget.DeleteTransactionAsync(transaction.Id);
            WeakReferenceMessenger.Default.Send(new DataChangedMessage());
        }
        catch (AppException ex)
        {
            if (ex.Kind == ErrorKind.SessionExpired)
            {
                await Shell.Current.DisplayAlert("Session expirée", "Reconnecte-toi pour continuer.", "OK");
                await Shell.Current.GoToAsync("//Login");
                return;
            }

            var message = ex.Kind switch
            {
                ErrorKind.Offline => "Pas de connexion internet. Vérifie ton réseau puis réessaie.",
                ErrorKind.Server => "Le serveur ne répond pas pour le moment. Réessaie dans quelques instants.",
                _ => ex.Message
            };
            await Shell.Current.DisplayAlert("Suppression impossible", message, "OK");
        }
    }

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
