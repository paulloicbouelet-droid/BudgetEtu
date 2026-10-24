using System.Globalization;
using System.Text.RegularExpressions;
using BudgetEtu.Models;
using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BudgetEtu.ViewModels;

/// <summary>
/// Formulaire d'une enveloppe de budget : création (sans paramètre) ou modification
/// (l'enveloppe est passée par la navigation sous la clé « Budget »).
/// </summary>
public partial class BudgetFormViewModel : FormViewModelBase, IQueryAttributable
{
    private static readonly string[] DefaultEmojis =
        { "🍔", "🚌", "🏠", "📱", "🎉", "💊", "📚", "👕", "🎁", "💰" };

    private readonly IBudgetService _budget;
    private Guid? _editingId;

    public BudgetFormViewModel(IBudgetService budget)
    {
        _budget = budget;
        emojis = DefaultEmojis.ToList();
        selectedEmoji = DefaultEmojis[0];
    }

    [ObservableProperty] private IList<string> emojis;
    [ObservableProperty] private string? selectedEmoji;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string limitText = string.Empty;

    [ObservableProperty] private string nameError = string.Empty;
    [ObservableProperty] private string limitError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    [NotifyPropertyChangedFor(nameof(SaveText))]
    private bool isEditing;

    public string PageTitle => IsEditing ? "Modifier l'enveloppe" : "Nouvelle enveloppe";
    public string SaveText => IsEditing ? "Enregistrer les modifications" : "Créer l'enveloppe";

    /// <summary>Reçoit l'enveloppe à modifier (absente = création).</summary>
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("Budget", out var value) || value is not BudgetCategory budget)
            return;

        _editingId = budget.Id;
        IsEditing = true;

        if (!Emojis.Contains(budget.Emoji))
            Emojis = Emojis.Prepend(budget.Emoji).ToList();

        SelectedEmoji = budget.Emoji;
        Name = budget.Name;
        LimitText = Math.Round(budget.Limit).ToString(CultureInfo.InvariantCulture);
    }

    [RelayCommand]
    private Task Close() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task Save()
    {
        NameError = LimitError = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
            NameError = "Donne un nom à l'enveloppe (ex. Nourriture).";

        decimal limit = 0;
        if (string.IsNullOrWhiteSpace(LimitText))
            LimitError = "Saisis un plafond mensuel.";
        else if (!TryParseAmount(LimitText, out limit))
            LimitError = "Montant invalide. Utilise uniquement des chiffres.";
        else if (limit <= 0)
            LimitError = "Le plafond doit être supérieur à 0 FCFA";

        if (NameError.Length + LimitError.Length > 0)
            return;

        await SubmitAsync(async () =>
        {
            var emoji = SelectedEmoji ?? "💰";

            if (_editingId is { } id)
                await _budget.UpdateBudgetAsync(id, emoji, Name, limit);
            else
                await _budget.CreateBudgetAsync(emoji, Name, limit);

            WeakReferenceMessenger.Default.Send(new DataChangedMessage());
            await Shell.Current.GoToAsync("..");
        });
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (_editingId is not { } id)
            return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Supprimer l'enveloppe",
            $"Supprimer l'enveloppe « {Name} » ? Tes transactions ne sont pas supprimées.",
            "Supprimer", "Annuler");

        if (!confirmed)
            return;

        await SubmitAsync(async () =>
        {
            await _budget.DeleteBudgetAsync(id);

            WeakReferenceMessenger.Default.Send(new DataChangedMessage());
            await Shell.Current.GoToAsync("..");
        });
    }

    /// <summary>Accepte « 50000 », « 50 000 », « 50 000 FCFA ».</summary>
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
