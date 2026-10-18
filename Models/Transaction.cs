using BudgetEtu.Services;

namespace BudgetEtu.Models;

public enum TransactionKind
{
    Expense,
    Income
}

public class Transaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public TransactionKind Kind { get; init; }
    public string Category { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Note { get; init; } = string.Empty;
    public DateTime Date { get; init; } = DateTime.Today;

    public bool IsExpense => Kind == TransactionKind.Expense;

    /// <summary>« - 1 000 FCFA » / « + 50 000 FCFA »</summary>
    public string AmountText => Fcfa.Signed(Amount, IsExpense);
}

public record DashboardData(decimal Income, decimal Expense, IReadOnlyList<Transaction> Recent)
{
    public decimal Balance => Income - Expense;
}
