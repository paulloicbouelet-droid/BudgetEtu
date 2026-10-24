using System.Globalization;
using BudgetEtu.Api.Data;
using BudgetEtu.Api.Models;
using BudgetEtu.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetEtu.Api.Controllers;

/// <summary>Enveloppes de budget (écran « Budgets »). La consommation est calculée à partir des transactions.</summary>
[ApiController]
[Route("api/budgets")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly BudgetCalculator _calculator;

    public BudgetsController(AppDbContext db, BudgetCalculator calculator)
    {
        _db = db;
        _calculator = calculator;
    }

    /// <summary>Budgets d'un mois : GET /api/budgets?month=2026-10 (mois courant par défaut).</summary>
    [HttpGet]
    public async Task<ActionResult<BudgetOverviewDto>> Get([FromQuery] string? month)
    {
        DateTime start;
        if (string.IsNullOrWhiteSpace(month))
        {
            var today = DateTime.Today;
            start = new DateTime(today.Year, today.Month, 1);
        }
        else if (!DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
        {
            return Problem("Mois invalide (format attendu : AAAA-MM).", statusCode: StatusCodes.Status400BadRequest);
        }

        var categories = await _calculator.ComputeAsync(User.GetUserId(), start);

        return Ok(new BudgetOverviewDto(
            start.ToString("yyyy-MM", CultureInfo.InvariantCulture),
            categories.Sum(c => c.Spent),
            categories.Sum(c => c.Limit),
            categories));
    }

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Create(SaveBudgetRequest request)
    {
        var error = Validate(request);
        if (error is not null)
            return Problem(error, statusCode: StatusCodes.Status400BadRequest);

        var userId = User.GetUserId();
        var lastOrder = await _db.Envelopes
            .Where(e => e.UserId == userId)
            .Select(e => (int?)e.SortOrder)
            .MaxAsync() ?? 0;

        var envelope = new BudgetEnvelope
        {
            UserId = userId,
            Emoji = string.IsNullOrWhiteSpace(request.Emoji) ? "💰" : request.Emoji.Trim(),
            Name = request.Name.Trim(),
            MonthlyLimit = request.MonthlyLimit,
            SortOrder = lastOrder + 1
        };

        _db.Envelopes.Add(envelope);
        await _db.SaveChangesAsync();

        return Created($"api/budgets/{envelope.Id}",
            new BudgetDto(envelope.Id, envelope.Emoji, envelope.Name, envelope.MonthlyLimit, 0));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, SaveBudgetRequest request)
    {
        var error = Validate(request);
        if (error is not null)
            return Problem(error, statusCode: StatusCodes.Status400BadRequest);

        var userId = User.GetUserId();
        var envelope = await _db.Envelopes.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (envelope is null)
            return NotFound();

        envelope.Emoji = string.IsNullOrWhiteSpace(request.Emoji) ? envelope.Emoji : request.Emoji.Trim();
        envelope.Name = request.Name.Trim();
        envelope.MonthlyLimit = request.MonthlyLimit;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        var envelope = await _db.Envelopes.FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        if (envelope is null)
            return NotFound();

        _db.Envelopes.Remove(envelope);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static string? Validate(SaveBudgetRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return "Donne un nom à l'enveloppe.";
        if (request.MonthlyLimit <= 0)
            return "Le plafond doit être supérieur à 0 FCFA.";
        return null;
    }
}
