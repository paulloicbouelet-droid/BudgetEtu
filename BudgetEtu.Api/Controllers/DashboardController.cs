using BudgetEtu.Api.Data;
using BudgetEtu.Api.Models;
using BudgetEtu.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetEtu.Api.Controllers;

/// <summary>Résumé de l'écran « Accueil » : solde, revenus, dépenses, 5 dernières transactions.</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _db;

    public DashboardController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get()
    {
        var userId = User.GetUserId();

        // Sommes en mémoire : SQLite ne sait pas additionner des décimaux côté base.
        var amounts = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => new { t.Kind, t.Amount })
            .ToListAsync();

        var income = amounts.Where(a => a.Kind == TransactionKind.Income).Sum(a => a.Amount);
        var expense = amounts.Where(a => a.Kind == TransactionKind.Expense).Sum(a => a.Amount);

        var recent = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Take(5)
            .ToListAsync();

        return Ok(new DashboardDto(income, expense, income - expense, recent.Select(t => t.ToDto()).ToList()));
    }
}
