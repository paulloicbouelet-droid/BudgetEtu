namespace BudgetEtu.Models;

public enum AlertLevel
{
    Info,
    Warning,
    Success
}

public class AlertItem
{
    public string Emoji { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string WhenText { get; init; } = string.Empty;
    public AlertLevel Level { get; init; }

    public string HeadingText => $"{Emoji} {Title}";
    public bool IsWarning => Level == AlertLevel.Warning;
    public bool IsSuccess => Level == AlertLevel.Success;
}
