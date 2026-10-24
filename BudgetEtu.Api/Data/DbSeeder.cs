using BudgetEtu.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BudgetEtu.Api.Data;

public static class DbSeeder
{
    /// <summary>Enveloppes créées pour chaque nouveau compte.</summary>
    public static List<BudgetEnvelope> DefaultEnvelopes() => new()
    {
        new() { Emoji = "🍔", Name = "Nourriture", MonthlyLimit = 50_000, SortOrder = 1 },
        new() { Emoji = "🚌", Name = "Transport", MonthlyLimit = 30_000, SortOrder = 2 },
        new() { Emoji = "🏠", Name = "Logement", MonthlyLimit = 20_000, SortOrder = 3 }
    };

    /// <summary>
    /// Compte de démonstration (créé seulement si la base est vide) :
    ///   e-mail : etudiant@univ.cm   mot de passe : budget123
    /// Ses données donnent les budgets des maquettes : Nourriture 45 000 / 50 000 (90 %),
    /// Transport 15 000 / 30 000, Logement 15 000 / 20 000.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher<User> hasher)
    {
        if (await db.Users.AnyAsync())
            return;

        var user = new User { FullName = "Paul Loïc", Email = "etudiant@univ.cm" };
        user.PasswordHash = hasher.HashPassword(user, "budget123");
        user.Envelopes.AddRange(DefaultEnvelopes());

        // Toutes les dates restent dans le mois courant (pour que les budgets du mois les comptent).
        var today = DateTime.Today;
        DateTime Day(int daysAgo) => new(today.Year, today.Month, Math.Max(1, today.Day - daysAgo));

        user.Transactions.AddRange(new[]
        {
            new Transaction { Kind = TransactionKind.Income, Category = "Bourse", Amount = 150_000, Note = "Bourse d'excellence", Date = Day(9) },
            new Transaction { Kind = TransactionKind.Expense, Category = "Logement", Amount = 15_000, Note = "Loyer de la chambre", Date = Day(7) },
            new Transaction { Kind = TransactionKind.Expense, Category = "Nourriture / Rations", Amount = 21_000, Note = "Courses de la semaine", Date = Day(8) },
            new Transaction { Kind = TransactionKind.Expense, Category = "Nourriture / Rations", Amount = 24_000, Note = "Achats au marché", Date = Day(3) },
            new Transaction { Kind = TransactionKind.Expense, Category = "Transport", Amount = 14_000, Note = "Abonnement bus", Date = Day(4) },
            new Transaction { Kind = TransactionKind.Income, Category = "Pension / Support", Amount = 50_000, Note = "Pension du mois", Date = Day(1) },
            new Transaction { Kind = TransactionKind.Expense, Category = "Transport", Amount = 1_000, Note = "Moto-taxi", Date = Day(0) }
        });

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
