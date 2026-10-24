namespace BudgetEtu.Api.Models;

public enum TransactionKind
{
    Expense = 0,
    Income = 1
}

public enum AlertLevel
{
    Info,
    Warning,
    Success
}

/// <summary>Compte utilisateur. Le mot de passe n'est jamais stocké en clair : seulement son empreinte (hash).</summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;

    /// <summary>Toujours en minuscules, unique.</summary>
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Empreinte du jeton « mot de passe oublié » (le jeton lui-même n'est jamais stocké).</summary>
    public string? ResetTokenHash { get; set; }
    public DateTime? ResetTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Transaction> Transactions { get; set; } = new();
    public List<BudgetEnvelope> Envelopes { get; set; } = new();
}

/// <summary>Une dépense ou un revenu d'un utilisateur.</summary>
public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public TransactionKind Kind { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Note { get; set; } = string.Empty;

    /// <summary>Date de la transaction (jour).</summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>Date d'enregistrement : sert à départager deux transactions du même jour.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Enveloppe de budget mensuelle (Nourriture, Transport…).</summary>
public class BudgetEnvelope
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Emoji { get; set; } = "💰";

    /// <summary>
    /// Nom de l'enveloppe. Les dépenses dont la catégorie COMMENCE par ce nom y sont comptées
    /// (« Nourriture / Rations » compte dans « Nourriture »).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public decimal MonthlyLimit { get; set; }
    public int SortOrder { get; set; }
}
