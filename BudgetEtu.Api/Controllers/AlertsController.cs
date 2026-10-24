using System.Globalization;
using BudgetEtu.Api.Data;
using BudgetEtu.Api.Models;
using BudgetEtu.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetEtu.Api.Controllers;

/// <summary>
/// Alertes (écran « Alertes »), calculées à la demande à partir des budgets et des transactions :
///  - enveloppe consommée à 80 % ou plus (« critique »), ou dépassée ;
///  - aucune saisie depuis 3 jours (rappel) ;
///  - moins de la moitié des revenus du mois dépensée (bonne nouvelle).
/// </summary>
[ApiController]
[Route("api/alerts")]
[Authorize]
public class AlertsController : ControllerBase
{
    private static readonly NumberFormatInfo Spaced = new() { NumberGroupSeparator = " " };

    private readonly AppDbContext _db;
    private readonly BudgetCalculator _calculator;

    public AlertsController(AppDbContext db, BudgetCalculator calculator)
    {
        _db = db;
        _calculator = calculator;
    }

    [HttpGet]
    public async Task<ActionResult<List<AlertDto>>> Get()
    {
        var userId = User.GetUserId();
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var alerts = new List<AlertDto>();

        // 1. Budgets critiques ou dépassés
        var budgets = await _calculator.ComputeAsync(userId, monthStart);
        foreach (var budget in budgets.Where(b => b.Limit > 0))
        {
            var ratio = budget.Spent / budget.Limit;
            if (ratio >= 1m)
            {
                var over = budget.Spent - budget.Limit;
                alerts.Add(new AlertDto(AlertLevel.Warning, "🚨", $"Budget {budget.Name} dépassé !",
                    $"Tu as dépassé ton enveloppe de {over.ToString("N0", Spaced)} FCFA.", now));
            }
            else if (ratio >= 0.8m)
            {
                alerts.Add(new AlertDto(AlertLevel.Warning, "⚠️", $"Budget {budget.Name} critique !",
                    $"Tu as consommé {(int)Math.Round(ratio * 100)}% de ton enveloppe.", now));
            }
        }

        // 2. Rappel de saisie
        var lastDate = await _db.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .Select(t => (DateTime?)t.Date)
            .FirstOrDefaultAsync();

        if (lastDate is null || (now.Date - lastDate.Value.Date).TotalDays >= 3)
        {
            alerts.Add(new AlertDto(AlertLevel.Info, "💡", "Rappel de saisie",
                "N'oublie pas de saisir tes dépenses.", now));
        }

        // 3. Bonne nouvelle : moins de la moitié des revenus du mois dépensée
        var monthAmounts = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && t.Date >= monthStart && t.Date < monthStart.AddMonths(1))
            .Select(t => new { t.Kind, t.Amount })
            .ToListAsync();

        var income = monthAmounts.Where(a => a.Kind == TransactionKind.Income).Sum(a => a.Amount);
        var expense = monthAmounts.Where(a => a.Kind == TransactionKind.Expense).Sum(a => a.Amount);

        if (income > 0 && expense * 2 <= income)
        {
            alerts.Add(new AlertDto(AlertLevel.Success, "🎉", "Épargne en bonne voie",
                "Tu as dépensé moins de la moitié de tes revenus ce mois-ci.", now));
        }

        // Avertissements d'abord, puis rappels, puis bonnes nouvelles.
        var ordered = alerts
            .OrderBy(a => a.Level == AlertLevel.Warning ? 0 : a.Level == AlertLevel.Info ? 1 : 2)
            .ToList();

        return Ok(ordered);
    }
}
