namespace BudgetEtu.Api.Models;

// DTO = ce qui circule dans les requêtes / réponses JSON (jamais les entités de la base directement).

// ----- Authentification -----
public record RegisterRequest(string FullName, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Token, string NewPassword);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record UserDto(Guid Id, string FullName, string Email);
public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

// ----- Transactions -----
public record TransactionDto(Guid Id, TransactionKind Kind, string Category, decimal Amount, string Note, DateTime Date);
public record CreateTransactionRequest(TransactionKind Kind, string Category, decimal Amount, string Note = "", DateTime? Date = null);

public record DashboardDto(decimal Income, decimal Expense, decimal Balance, List<TransactionDto> Recent);

// ----- Budgets -----
public record BudgetDto(Guid Id, string Emoji, string Name, decimal Limit, decimal Spent);
public record BudgetOverviewDto(string Month, decimal TotalSpent, decimal TotalLimit, List<BudgetDto> Categories);
public record SaveBudgetRequest(string Emoji, string Name, decimal MonthlyLimit);

// ----- Alertes -----
public record AlertDto(AlertLevel Level, string Emoji, string Title, string Message, DateTime CreatedAt);

public static class Mapping
{
    public static TransactionDto ToDto(this Transaction t) =>
        new(t.Id, t.Kind, t.Category, t.Amount, t.Note, t.Date);
}
