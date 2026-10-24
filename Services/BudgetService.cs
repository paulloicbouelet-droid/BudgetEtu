using BudgetEtu.Models;

namespace BudgetEtu.Services;

public interface IBudgetService
{
    IReadOnlyList<string> Months { get; }
    IReadOnlyList<string> ExpenseCategories { get; }
    IReadOnlyList<string> IncomeCategories { get; }

    Task<DashboardData> GetDashboardAsync();
    Task<BudgetOverview> GetBudgetsAsync(int monthIndex);
    Task<IReadOnlyList<AlertItem>> GetAlertsAsync();
    Task AddTransactionAsync(Transaction transaction);
    Task DeleteTransactionAsync(Guid id);

    Task CreateBudgetAsync(string emoji, string name, decimal monthlyLimit);
    Task UpdateBudgetAsync(Guid id, string emoji, string name, decimal monthlyLimit);
    Task DeleteBudgetAsync(Guid id);
}

/// <summary>
/// Données fictives en mémoire, calquées sur les maquettes
/// (solde 150 000 = 200 000 de revenus - 50 000 de dépenses ; budgets Nourriture / Transport…).
/// À remplacer par un appel à ton backend.
/// </summary>
public class BudgetService : IBudgetService
{
    public IReadOnlyList<string> Months { get; } = new[]
    {
        "Octobre 2026", "Septembre 2026", "Août 2026", "Juillet 2026", "Juin 2026", "Mai 2026"
    };

    public IReadOnlyList<string> ExpenseCategories { get; } = new[]
    {
        "Nourriture / Rations", "Transport", "Logement", "Internet / Téléphone",
        "Loisirs", "Santé", "Études / Fournitures", "Autre"
    };

    public IReadOnlyList<string> IncomeCategories { get; } = new[]
    {
        "Pension / Support", "Bourse", "Job étudiant", "Autre revenu"
    };

    // Plus récent en premier
    private readonly List<Transaction> _transactions = new()
    {
        new() { Kind = TransactionKind.Expense, Category = "Transport", Amount = 1_000, Note = "Moto-taxi", Date = DateTime.Today },
        new() { Kind = TransactionKind.Income, Category = "Pension / Support", Amount = 50_000, Note = "Pension du mois", Date = DateTime.Today.AddDays(-1) },
        new() { Kind = TransactionKind.Expense, Category = "Nourriture / Rations", Amount = 24_000, Note = "Achats au marché", Date = DateTime.Today.AddDays(-3) },
        new() { Kind = TransactionKind.Expense, Category = "Loisirs", Amount = 15_000, Note = "Sortie entre amis", Date = DateTime.Today.AddDays(-5) },
        new() { Kind = TransactionKind.Expense, Category = "Internet / Téléphone", Amount = 10_000, Note = "Forfait data", Date = DateTime.Today.AddDays(-6) },
        new() { Kind = TransactionKind.Income, Category = "Bourse", Amount = 150_000, Note = "Bourse d'excellence", Date = DateTime.Today.AddDays(-9) }
    };

    // Mois courant : 45 000 + 15 000 + 15 000 = 75 000 dépensés sur 100 000 (comme dans la maquette 5_Budgets).
    private readonly List<BudgetCategory> _currentBudgets = new()
    {
        new() { Emoji = "🍔", Name = "Nourriture", Spent = 45_000, Limit = 50_000 },
        new() { Emoji = "🚌", Name = "Transport", Spent = 15_000, Limit = 30_000 },
        new() { Emoji = "🏠", Name = "Logement", Spent = 15_000, Limit = 20_000 }
    };

    private readonly List<AlertItem> _alerts = new()
    {
        new() { Emoji = "⚠️", Title = "Budget Nourriture critique !", Message = "Tu as consommé 90% de ton enveloppe.", WhenText = "Aujourd'hui, 14:30", Level = AlertLevel.Warning },
        new() { Emoji = "💡", Title = "Rappel de fin de semaine", Message = "N'oublie pas de saisir tes dépenses.", WhenText = "Hier, 18:00", Level = AlertLevel.Info },
        new() { Emoji = "🎉", Title = "Objectif Épargne atteint", Message = "Tu as économisé 5 000 FCFA !", WhenText = "04 Oct, 10:15", Level = AlertLevel.Success }
    };

    public async Task<DashboardData> GetDashboardAsync()
    {
        await Simulation.EnterAsync();

        if (Simulation.Scenario == DemoScenario.Empty)
            return new DashboardData(0, 0, Array.Empty<Transaction>());

        var income = _transactions.Where(t => !t.IsExpense).Sum(t => t.Amount);
        var expense = _transactions.Where(t => t.IsExpense).Sum(t => t.Amount);
        var recent = _transactions.OrderByDescending(t => t.Date).Take(5).ToList();

        return new DashboardData(income, expense, recent);
    }

    public async Task<BudgetOverview> GetBudgetsAsync(int monthIndex)
    {
        await Simulation.EnterAsync();

        if (Simulation.Scenario == DemoScenario.Empty)
            return new BudgetOverview(Array.Empty<BudgetCategory>());

        // Mois courant : données réelles. Mois précédents : valeurs fictives déterministes.
        var categories = _currentBudgets.Select((b, i) => new BudgetCategory
        {
            Id = b.Id,
            Emoji = b.Emoji,
            Name = b.Name,
            Limit = b.Limit,
            Spent = monthIndex == 0
                ? b.Spent
                : Math.Round(b.Limit * (decimal)(0.55 + 0.07 * ((monthIndex * 3 + i * 2) % 5)) / 500m) * 500m
        }).ToList();

        return new BudgetOverview(categories);
    }

    public async Task<IReadOnlyList<AlertItem>> GetAlertsAsync()
    {
        await Simulation.EnterAsync();

        return Simulation.Scenario == DemoScenario.Empty
            ? Array.Empty<AlertItem>()
            : _alerts.ToList();
    }

    public async Task AddTransactionAsync(Transaction transaction)
    {
        await Simulation.EnterAsync();

        // Si la démo était en mode « aucune donnée », la nouvelle saisie la fait réapparaître.
        if (Simulation.Scenario == DemoScenario.Empty)
            Simulation.Scenario = DemoScenario.Normal;

        _transactions.Insert(0, transaction);

        if (transaction.IsExpense)
        {
            var budget = _currentBudgets.FirstOrDefault(b =>
                transaction.Category.StartsWith(b.Name, StringComparison.OrdinalIgnoreCase));
            if (budget is not null)
                budget.Spent += transaction.Amount;
        }
    }

    public async Task DeleteTransactionAsync(Guid id)
    {
        await Simulation.EnterAsync();

        var transaction = _transactions.FirstOrDefault(t => t.Id == id);
        if (transaction is null)
            throw new AppException(ErrorKind.Validation, "Cette transaction n'existe plus.");

        _transactions.Remove(transaction);

        // Dépense du mois courant : on rend l'argent à l'enveloppe correspondante.
        var today = DateTime.Today;
        if (transaction.IsExpense && transaction.Date.Year == today.Year && transaction.Date.Month == today.Month)
        {
            var budget = FindBudget(transaction.Category);
            if (budget is not null)
                budget.Spent = Math.Max(0, budget.Spent - transaction.Amount);
        }
    }

    public async Task CreateBudgetAsync(string emoji, string name, decimal monthlyLimit)
    {
        await Simulation.EnterAsync();

        if (Simulation.Scenario == DemoScenario.Empty)
            Simulation.Scenario = DemoScenario.Normal;

        _currentBudgets.Add(new BudgetCategory
        {
            Emoji = string.IsNullOrWhiteSpace(emoji) ? "💰" : emoji.Trim(),
            Name = name.Trim(),
            Limit = monthlyLimit,
            Spent = SpentThisMonth(name.Trim())
        });
    }

    public async Task UpdateBudgetAsync(Guid id, string emoji, string name, decimal monthlyLimit)
    {
        await Simulation.EnterAsync();

        var index = _currentBudgets.FindIndex(b => b.Id == id);
        if (index < 0)
            throw new AppException(ErrorKind.Validation, "Cette enveloppe n'existe plus.");

        var old = _currentBudgets[index];
        var cleanName = name.Trim();

        _currentBudgets[index] = new BudgetCategory
        {
            Id = old.Id,
            Emoji = string.IsNullOrWhiteSpace(emoji) ? old.Emoji : emoji.Trim(),
            Name = cleanName,
            Limit = monthlyLimit,
            Spent = string.Equals(old.Name, cleanName, StringComparison.OrdinalIgnoreCase)
                ? old.Spent
                : SpentThisMonth(cleanName)
        };
    }

    public async Task DeleteBudgetAsync(Guid id)
    {
        await Simulation.EnterAsync();

        if (_currentBudgets.RemoveAll(b => b.Id == id) == 0)
            throw new AppException(ErrorKind.Validation, "Cette enveloppe n'existe plus.");
    }

    private BudgetCategory? FindBudget(string transactionCategory)
        => _currentBudgets.FirstOrDefault(b =>
            transactionCategory.StartsWith(b.Name, StringComparison.OrdinalIgnoreCase));

    private decimal SpentThisMonth(string budgetName)
    {
        var today = DateTime.Today;
        return _transactions
            .Where(t => t.IsExpense && t.Date.Year == today.Year && t.Date.Month == today.Month
                        && t.Category.StartsWith(budgetName, StringComparison.OrdinalIgnoreCase))
            .Sum(t => t.Amount);
    }
}
