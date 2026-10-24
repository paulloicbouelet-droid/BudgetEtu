using System.Globalization;
using BudgetEtu.Models;

namespace BudgetEtu.Services;

/// <summary>
/// Données réelles : appelle /api/dashboard, /api/budgets, /api/alerts et /api/transactions du serveur BudgetEtu.Api.
/// Les budgets et les alertes sont calculés par le serveur à partir des transactions enregistrées.
/// </summary>
public class ApiBudgetService : IBudgetService
{
    private static readonly CultureInfo French = CultureInfo.GetCultureInfo("fr-FR");

    private readonly ApiClient _api;

    public ApiBudgetService(ApiClient api)
    {
        _api = api;
    }

    /// <summary>Les 6 derniers mois, le mois courant en premier (« Octobre 2026 », « Septembre 2026 »…).</summary>
    public IReadOnlyList<string> Months { get; } = BuildMonths();

    public IReadOnlyList<string> ExpenseCategories { get; } = new[]
    {
        "Nourriture / Rations", "Transport", "Logement", "Internet / Téléphone",
        "Loisirs", "Santé", "Études / Fournitures", "Autre"
    };

    public IReadOnlyList<string> IncomeCategories { get; } = new[]
    {
        "Pension / Support", "Bourse", "Job étudiant", "Autre revenu"
    };

    public async Task<DashboardData> GetDashboardAsync()
    {
        await Simulation.EnterAsync();

        if (Simulation.Scenario == DemoScenario.Empty)
            return new DashboardData(0, 0, Array.Empty<Transaction>());

        var dto = await _api.GetAsync<ApiDashboard>("api/dashboard");
        return new DashboardData(dto.Income, dto.Expense, dto.Recent.Select(ToModel).ToList());
    }

    public async Task<BudgetOverview> GetBudgetsAsync(int monthIndex)
    {
        await Simulation.EnterAsync();

        if (Simulation.Scenario == DemoScenario.Empty)
            return new BudgetOverview(Array.Empty<BudgetCategory>());

        var month = DateTime.Today.AddMonths(-Math.Max(0, monthIndex)).ToString("yyyy-MM", CultureInfo.InvariantCulture);
        var dto = await _api.GetAsync<ApiBudgetOverview>($"api/budgets?month={month}");

        var categories = dto.Categories
            .Select(b => new BudgetCategory { Emoji = b.Emoji, Name = b.Name, Limit = b.Limit, Spent = b.Spent })
            .ToList();

        return new BudgetOverview(categories);
    }

    public async Task<IReadOnlyList<AlertItem>> GetAlertsAsync()
    {
        await Simulation.EnterAsync();

        if (Simulation.Scenario == DemoScenario.Empty)
            return Array.Empty<AlertItem>();

        var alerts = await _api.GetAsync<List<ApiAlert>>("api/alerts");

        return alerts
            .Select(a => new AlertItem
            {
                Emoji = a.Emoji,
                Title = a.Title,
                Message = a.Message,
                WhenText = DescribeWhen(a.CreatedAt),
                Level = a.Level
            })
            .ToList();
    }

    public async Task AddTransactionAsync(Transaction transaction)
    {
        await Simulation.EnterAsync();

        // Si la démo était en mode « aucune donnée », la nouvelle saisie la fait réapparaître.
        if (Simulation.Scenario == DemoScenario.Empty)
            Simulation.Scenario = DemoScenario.Normal;

        await _api.PostAsync("api/transactions", new
        {
            kind = transaction.Kind,
            category = transaction.Category,
            amount = transaction.Amount,
            note = transaction.Note,
            date = transaction.Date.Date
        });
    }

    // ----- Conversions -----

    private static Transaction ToModel(ApiTransaction t) => new()
    {
        Id = t.Id,
        Kind = t.Kind,
        Category = t.Category,
        Amount = t.Amount,
        Note = t.Note,
        Date = t.Date
    };

    private static string DescribeWhen(DateTime when) =>
        when.Date == DateTime.Today
            ? $"Aujourd'hui, {when:HH:mm}"
            : when.ToString("dd MMM, HH:mm", French);

    private static IReadOnlyList<string> BuildMonths()
    {
        var first = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        return Enumerable.Range(0, 6)
            .Select(i =>
            {
                var text = first.AddMonths(-i).ToString("MMMM yyyy", French);
                return char.ToUpper(text[0], French) + text[1..];
            })
            .ToList();
    }
}
