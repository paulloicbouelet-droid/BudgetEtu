using BudgetEtu.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BudgetEtu.ViewModels;

/// <summary>Message envoyé quand les données changent (nouvelle transaction, changement de scénario de démo…).</summary>
public sealed record DataChangedMessage;

public enum ViewState
{
    Loading,
    Content,
    Empty,
    Offline,
    ServerError,
    SessionExpired
}

/// <summary>
/// Base des écrans qui chargent des données (Accueil, Budgets, Alertes).
/// Gère les 6 états d'un écran : chargement, contenu, vide, hors ligne, erreur serveur, session expirée.
/// Chaque écran fournit seulement <see cref="FetchAsync"/> et les textes de son état « vide ».
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    protected ViewModelBase()
    {
        // Recharge l'écran quand les données changent ailleurs dans l'application.
        WeakReferenceMessenger.Default.Register<ViewModelBase, DataChangedMessage>(
            this,
            static (recipient, _) => MainThread.BeginInvokeOnMainThread(() => recipient.LoadCommand.Execute(null)));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    [NotifyPropertyChangedFor(nameof(IsContent))]
    [NotifyPropertyChangedFor(nameof(IsProblem))]
    [NotifyPropertyChangedFor(nameof(ProblemIcon))]
    [NotifyPropertyChangedFor(nameof(ProblemTitle))]
    [NotifyPropertyChangedFor(nameof(ProblemMessage))]
    [NotifyPropertyChangedFor(nameof(ProblemActionText))]
    private ViewState state = ViewState.Loading;

    [ObservableProperty]
    private bool isRefreshing;

    public bool IsLoading => State == ViewState.Loading;
    public bool IsContent => State == ViewState.Content;
    public bool IsProblem => State is not (ViewState.Loading or ViewState.Content);

    // ----- Textes de l'écran d'erreur / état vide -----

    public string ProblemIcon => State switch
    {
        ViewState.Empty => EmptyIcon,
        ViewState.Offline => "📡",
        ViewState.SessionExpired => "🔒",
        _ => "🛠️"
    };

    public string ProblemTitle => State switch
    {
        ViewState.Empty => EmptyTitle,
        ViewState.Offline => "Pas de connexion",
        ViewState.SessionExpired => "Session expirée",
        _ => "Oups, un problème est survenu"
    };

    public string ProblemMessage => State switch
    {
        ViewState.Empty => EmptyMessage,
        ViewState.Offline => "Vérifie ta connexion internet puis réessaie.",
        ViewState.SessionExpired => "Pour protéger tes données, reconnecte-toi pour continuer.",
        _ => "Nos serveurs ne répondent pas pour le moment. Réessaie dans quelques instants."
    };

    public string ProblemActionText => State switch
    {
        ViewState.Empty => EmptyActionText,
        ViewState.SessionExpired => "Se reconnecter",
        _ => "Réessayer"
    };

    protected abstract string EmptyIcon { get; }
    protected abstract string EmptyTitle { get; }
    protected abstract string EmptyMessage { get; }

    /// <summary>Texte du bouton de l'état vide ; vide = pas de bouton.</summary>
    protected virtual string EmptyActionText => string.Empty;

    /// <summary>Charge les données. Retourne false si le résultat est vide.</summary>
    protected abstract Task<bool> FetchAsync();

    protected virtual Task OnEmptyActionAsync() => Task.CompletedTask;

    // ----- Commandes -----

    [RelayCommand]
    private Task Load() => LoadCoreAsync(showLoading: State != ViewState.Content);

    [RelayCommand]
    private async Task Refresh()
    {
        IsRefreshing = true;
        try
        {
            await LoadCoreAsync(showLoading: false);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>Bouton de l'écran d'erreur : « Réessayer », « Se reconnecter » ou l'action de l'état vide.</summary>
    [RelayCommand]
    private async Task RunProblemAction()
    {
        switch (State)
        {
            case ViewState.Empty:
                await OnEmptyActionAsync();
                break;
            case ViewState.SessionExpired:
                await Shell.Current.GoToAsync("//Login");
                break;
            default:
                await LoadCoreAsync(showLoading: true);
                break;
        }
    }

    private async Task LoadCoreAsync(bool showLoading)
    {
        if (showLoading)
            State = ViewState.Loading;

        try
        {
            State = await FetchAsync() ? ViewState.Content : ViewState.Empty;
        }
        catch (AppException ex)
        {
            State = ex.Kind switch
            {
                ErrorKind.Offline => ViewState.Offline,
                ErrorKind.SessionExpired => ViewState.SessionExpired,
                _ => ViewState.ServerError
            };
        }
        catch (Exception)
        {
            State = ViewState.ServerError;
        }
    }
}
