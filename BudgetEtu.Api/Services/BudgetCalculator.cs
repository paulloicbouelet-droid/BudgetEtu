using BudgetEtu.Api.Data;
using BudgetEtu.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetEtu.Api.Services;

/// <summary>Calcule la consommation de chaque enveloppe pour un mois (utilisé par les budgets ET par les alertes).</summary>
public class BudgetCalculator
{
    private readonly AppDbContext _db;

    public BudgetCalculator(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<BudgetDto>> ComputeAsync(Guid userId, DateTime monthStart)
    {
        var monthEnd = monthStart.AddMonths(1);

        var envelopes = await _db.Envelopes
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.SortOrder)
            .ThenBy(e => e.Name)
            .ToListAsync();

        // Les sommes de décimaux se font en mémoire : SQLite ne sait pas les additionner côté base.
        var expenses = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId
                        && t.Kind == TransactionKind.Expense
                        && t.Date >= monthStart
                        && t.Date < monthEnd)
            .Select(t => new { t.Category, t.Amount })
            .ToListAsync();

        return envelopes
            .Select(e => new BudgetDto(
                e.Id,
                e.Emoji,
                e.Name,
                e.MonthlyLimit,
                expenses
                    .Where(x => x.Category.StartsWith(e.Name, StringComparison.OrdinalIgnoreCase))
                    .Sum(x => x.Amount)))
            .ToList();
    }
}
