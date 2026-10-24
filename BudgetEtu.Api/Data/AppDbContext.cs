using BudgetEtu.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BudgetEtu.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<BudgetEnvelope> Envelopes => Set<BudgetEnvelope>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Un e-mail = un seul compte.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Supprimer un utilisateur supprime ses transactions et ses enveloppes.
        modelBuilder.Entity<User>()
            .HasMany(u => u.Transactions)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Envelopes)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
