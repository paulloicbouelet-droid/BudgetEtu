namespace BudgetEtu.Controls;

public partial class BottomNavBar : ContentView
{
    /// <summary>Route de l'onglet actif : Dashboard, Budgets, Alerts ou Profile.</summary>
    public static readonly BindableProperty ActiveTabProperty =
        BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(BottomNavBar), string.Empty,
            propertyChanged: (b, _, _) => ((BottomNavBar)b).UpdateTabs());

    public BottomNavBar()
    {
        InitializeComponent();
        UpdateTabs();
    }

    public string ActiveTab
    {
        get => (string)GetValue(ActiveTabProperty);
        set => SetValue(ActiveTabProperty, value);
    }

    private void UpdateTabs()
    {
        ApplyTabStyle(HomeTab, "Dashboard");
        ApplyTabStyle(BudgetTab, "Budgets");
        ApplyTabStyle(AlertsTab, "Alerts");
        ApplyTabStyle(ProfileTab, "Profile");
    }

    private void ApplyTabStyle(Label tab, string route)
    {
        var active = route == ActiveTab;
        tab.TextColor = ResourceHelper.Color(active ? "Primary" : "TextMuted");
        tab.FontFamily = ResourceHelper.Font(active ? "FontBold" : "FontBody");
    }

    private async void OnTabTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not string route || route == ActiveTab)
            return;

        await Shell.Current.GoToAsync($"//{route}");
    }

    private async void OnAddTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("Transaction");
}
