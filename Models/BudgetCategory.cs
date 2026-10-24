using BudgetEtu.Services;

namespace BudgetEtu.Models;

public class BudgetCategory
{
    /// <summary>Identifiant de l'enveloppe (sert à la modifier / la supprimer).</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Emoji { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal Spent { get; set; }
    public decimal Limit { get; init; }

    public decimal Remaining => Limit - Spent;
    public double Ratio => Limit <= 0 ? 0 : (double)(Spent / Limit);

    /// <summary>Valeur 0..1 pour la barre de progression.</summary>
    public double Progress => Math.Min(1d, Ratio);

    /// <summary>Vrai à partir de 80 % de l'enveloppe consommée.</summary>
    public bool IsWarning => Ratio >= 0.8;

    public string Title => $"{Emoji} {Name}";

    /// <summary>« 45 000 / 50 000 FCFA »</summary>
    public string SpentText => $"{Fcfa.Number(Spent)} / {Fcfa.Amount(Limit)}";

    public string RemainingText => Remaining >= 0
        ? $"Reste : {Fcfa.Amount(Remaining)}"
        : $"Dépassé de {Fcfa.Amount(-Remaining)}";
}

public record BudgetOverview(IReadOnlyList<BudgetCategory> Categories)
{
    public decimal TotalSpent => Categories.Sum(c => c.Spent);
    public decimal TotalLimit => Categories.Sum(c => c.Limit);
    public double Progress => TotalLimit <= 0 ? 0 : Math.Min(1d, (double)(TotalSpent / TotalLimit));
    public bool IsWarning => TotalLimit > 0 && (double)(TotalSpent / TotalLimit) >= 0.8;
}
