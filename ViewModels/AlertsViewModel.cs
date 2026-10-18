using System.Collections.ObjectModel;
using BudgetEtu.Models;
using BudgetEtu.Services;

namespace BudgetEtu.ViewModels;

public partial class AlertsViewModel : ViewModelBase
{
    private readonly IBudgetService _budget;

    public AlertsViewModel(IBudgetService budget)
    {
        _budget = budget;
    }

    public ObservableCollection<AlertItem> Alerts { get; } = new();

    protected override string EmptyIcon => "🔔";
    protected override string EmptyTitle => "Aucune alerte";
    protected override string EmptyMessage => "Tout va bien ! Tu seras prévenu dès qu'un budget approche de sa limite.";

    protected override async Task<bool> FetchAsync()
    {
        var alerts = await _budget.GetAlertsAsync();

        Alerts.Clear();
        foreach (var alert in alerts)
            Alerts.Add(alert);

        return alerts.Count > 0;
    }
}
