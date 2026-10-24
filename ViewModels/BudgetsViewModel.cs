using System.Collections.ObjectModel;
using BudgetEtu.Models;
using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetEtu.ViewModels;

public partial class BudgetsViewModel : ViewModelBase
{
    private readonly IBudgetService _budget;

    public BudgetsViewModel(IBudgetService budget)
    {
        _budget = budget;
        Months = budget.Months.ToList();
        selectedMonth = Months[0]; // champ direct : évite de déclencher un chargement dans le constructeur
    }

    public IList<string> Months { get; }

    [ObservableProperty] private string selectedMonth;

    [ObservableProperty] private string totalText = string.Empty;
    [ObservableProperty] private string totalRemainingText = string.Empty;
    [ObservableProperty] private double totalProgress;
    [ObservableProperty] private bool totalIsWarning;

    public ObservableCollection<BudgetCategory> Categories { get; } = new();

    protected override string EmptyIcon => "📊";
    protected override string EmptyTitle => "Aucun budget défini";
    protected override string EmptyMessage => "Tes budgets par catégorie apparaîtront ici dès que tu en auras créé.";
    protected override string EmptyActionText => "Créer une enveloppe";

    protected override Task OnEmptyActionAsync() => Shell.Current.GoToAsync("BudgetForm");

    [RelayCommand]
    private Task AddBudget() => Shell.Current.GoToAsync("BudgetForm");

    /// <summary>Touche sur une enveloppe : ouvre le formulaire pour la modifier ou la supprimer.</summary>
    [RelayCommand]
    private Task EditBudget(BudgetCategory category)
        => Shell.Current.GoToAsync("BudgetForm", new ShellNavigationQueryParameters { ["Budget"] = category });

    partial void OnSelectedMonthChanged(string value) => LoadCommand.Execute(null);

    protected override async Task<bool> FetchAsync()
    {
        var overview = await _budget.GetBudgetsAsync(Months.IndexOf(SelectedMonth));

        TotalText = $"Total dépensé : {Fcfa.Number(overview.TotalSpent)} / {Fcfa.Amount(overview.TotalLimit)}";
        var remaining = overview.TotalLimit - overview.TotalSpent;
        TotalRemainingText = remaining >= 0
            ? $"Reste : {Fcfa.Amount(remaining)}"
            : $"Dépassé de {Fcfa.Amount(-remaining)}";
        TotalProgress = overview.Progress;
        TotalIsWarning = overview.IsWarning;

        Categories.Clear();
        foreach (var category in overview.Categories)
            Categories.Add(category);

        return overview.Categories.Count > 0;
    }
}
