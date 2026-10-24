using BudgetEtu.Models;

namespace BudgetEtu.Services;

// Formes des réponses JSON du serveur (miroir de BudgetEtu.Api/Models/Dtos.cs).
// L'application les convertit ensuite en modèles d'affichage (Transaction, BudgetCategory, AlertItem…).

public record ApiUser(Guid Id, string FullName, string Email);

public record ApiAuthResponse(string Token, DateTime ExpiresAt, ApiUser User);

public record ApiTransaction(Guid Id, TransactionKind Kind, string Category, decimal Amount, string Note, DateTime Date);

public record ApiDashboard(decimal Income, decimal Expense, decimal Balance, List<ApiTransaction> Recent);

public record ApiBudget(Guid Id, string Emoji, string Name, decimal Limit, decimal Spent);

public record ApiBudgetOverview(string Month, decimal TotalSpent, decimal TotalLimit, List<ApiBudget> Categories);

public record ApiAlert(AlertLevel Level, string Emoji, string Title, string Message, DateTime CreatedAt);

/// <summary>Corps d'erreur renvoyé par le serveur (« application/problem+json »).</summary>
public record ApiProblem(string? Detail, string? Title);
