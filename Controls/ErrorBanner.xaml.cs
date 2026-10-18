namespace BudgetEtu.Controls;

public partial class ErrorBanner : ContentView
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(ErrorBanner), string.Empty,
            propertyChanged: (b, _, _) => ((ErrorBanner)b).UpdateVisibility());

    public static readonly BindableProperty IsSuccessProperty =
        BindableProperty.Create(nameof(IsSuccess), typeof(bool), typeof(ErrorBanner), false,
            propertyChanged: (b, _, _) => ((ErrorBanner)b).ApplyStyle());

    public ErrorBanner()
    {
        InitializeComponent();
        ApplyStyle();
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>Vrai pour un message de confirmation (vert) plutôt qu'une erreur (rouge).</summary>
    public bool IsSuccess
    {
        get => (bool)GetValue(IsSuccessProperty);
        set => SetValue(IsSuccessProperty, value);
    }

    private void UpdateVisibility() => IsVisible = !string.IsNullOrWhiteSpace(Message);

    private void ApplyStyle()
    {
        var accent = IsSuccess ? ResourceHelper.Color("Primary") : ResourceHelper.Color("Error");
        var soft = IsSuccess ? ResourceHelper.Color("PrimarySoft") : ResourceHelper.Color("ErrorSoft");

        Box.BackgroundColor = soft;
        Box.Stroke = new SolidColorBrush(accent);
        MessageLabel.TextColor = accent;
        IconLabel.Text = IsSuccess ? "✅" : "⚠️";
    }
}
