using System.Globalization;
using System.Text.RegularExpressions;
using BudgetEtu.Models;
using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BudgetEtu.ViewModels;

public partial class TransactionViewModel : FormViewModelBase
{
    private readonly IBudgetService _budget;

    public TransactionViewModel(IBudgetService budget)
    {
        _budget = budget;
        categories = budget.ExpenseCategories.ToList();
        selectedCategory = categories[0]; // « Nourriture / Rations », comme dans la maquette
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIncome))]
    private bool isExpense = true;

    public bool IsIncome => !IsExpense;

    [ObservableProperty] private IList<string> categories;
    [ObservableProperty] private string? selectedCategory;
    [ObservableProperty] private string amountText = string.Empty;
    [ObservableProperty] private string note = string.Empty;
    [ObservableProperty] private DateTime date = DateTime.Today;

    [ObservableProperty] private string amountError = string.Empty;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasCategoryError))]
    private string categoryError = string.Empty;

    [ObservableProperty] private string noteError = string.Empty;

    public bool HasCategoryError => CategoryError.Length > 0;

    partial void OnIsExpenseChanged(bool value)
    {
        Categories = (value ? _budget.ExpenseCategories : _budget.IncomeCategories).ToList();
        SelectedCategory = Categories[0];
    }

    [RelayCommand]
    private void SelectExpense() => IsExpense = true;

    [RelayCommand]
    private void SelectIncome() => IsExpense = false;

    [RelayCommand]
    private Task Close() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task Save()
    {
        AmountError = CategoryError = NoteError = string.Empty;

        // Montant : cas de la maquette « 4_SaisieTransaction_Erreur » (ex. « -2 500 FCFA »)
        decimal amount = 0;
        if (string.IsNullOrWhiteSpace(AmountText))
            AmountError = "Saisis un montant.";
        else if (!TryParseAmount(AmountText, out amount))
            AmountError = "Montant invalide. Utilise uniquement des chiffres.";
        else if (amount <= 0)
            AmountError = "Le montant doit être supérieur à 0 FCFA";

        if (string.IsNullOrWhiteSpace(SelectedCategory))
            CategoryError = "Choisis une catégorie.";

        if (string.IsNullOrWhiteSpace(Note))
            NoteError = "Ajoute un motif (ex. Achats au marché).";

        if (AmountError.Length + CategoryError.Length + NoteError.Length > 0)
            return;

        await SubmitAsync(async () =>
        {
            await _budget.AddTransactionAsync(new Transaction
            {
                Kind = IsExpense ? TransactionKind.Expense : TransactionKind.Income,
                Category = SelectedCategory!,
                Amount = amount,
                Note = Note.Trim(),
                Date = Date
            });

            WeakReferenceMessenger.Default.Send(new DataChangedMessage());

            // Schéma de navigation : la saisie rapide se termine par un retour à l'accueil (3_TableauDeBord).
            await Shell.Current.GoToAsync("//Dashboard");
        });
    }

    /// <summary>Accepte « 2500 », « 2 500 », « 2 500 FCFA », « -2 500 FCFA », « 2,5 ».</summary>
    private static bool TryParseAmount(string text, out decimal amount)
    {
        var cleaned = Regex.Replace(text, @"[^\d,.\-]", string.Empty).Replace(',', '.');
        return decimal.TryParse(
            cleaned,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out amount);
    }
}
