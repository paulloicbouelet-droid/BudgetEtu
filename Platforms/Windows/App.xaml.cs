using Microsoft.UI.Xaml;

namespace BudgetEtu.WinUI;

/// <summary>Point d'entrée de l'application Windows (WinUI 3).</summary>
public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
