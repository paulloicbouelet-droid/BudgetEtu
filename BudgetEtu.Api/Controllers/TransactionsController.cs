using BudgetEtu.Api.Data;
using BudgetEtu.Api.Models;
using BudgetEtu.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetEtu.Api.Controllers;

/// <summary>Dépenses et revenus de l'utilisateur connecté (écran « Saisie » et liste des transactions).</summary>
[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public TransactionsController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Les transactions, de la plus récente à la plus ancienne.</summary>
    [HttpGet]
    public async Task<ActionResult<List<TransactionDto>>> List([FromQuery] int take = 50)
    {
        var userId = User.GetUserId();
        take = Math.Clamp(take, 1, 200);

        var items = await _db.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Take(take)
            .ToListAsync();

        return Ok(items.Select(t => t.ToDto()).ToList());
    }

    /// <summary>Enregistre une transaction. Messages d'erreur identiques à ceux de la maquette « Saisie_Erreur ».</summary>
    [HttpPost]
    public async Task<ActionResult<TransactionDto>> Create(CreateTransactionRequest request)
    {
        if (request.Amount <= 0)
            return Problem("Le montant doit être supérieur à 0 FCFA", statusCode: StatusCodes.Status400BadRequest);
        if (string.IsNullOrWhiteSpace(request.Category))
            return Problem("Choisis une catégorie.", statusCode: StatusCodes.Status400BadRequest);

        var transaction = new Transaction
        {
            UserId = User.GetUserId(),
            Kind = request.Kind,
            Category = request.Category.Trim(),
            Amount = request.Amount,
            Note = (request.Note ?? string.Empty).Trim(),
            Date = (request.Date ?? DateTime.Today).Date
        };

        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();

        return Created($"api/transactions/{transaction.Id}", transaction.ToDto());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        var transaction = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
        if (transaction is null)
            return NotFound();

        _db.Transactions.Remove(transaction);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
