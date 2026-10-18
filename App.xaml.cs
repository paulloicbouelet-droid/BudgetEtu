namespace BudgetEtu;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Les maquettes sont conçues en thème clair uniquement.
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

#if WINDOWS
        // Sur PC, la fenêtre prend le format d'un téléphone pour juger les maquettes.
        window.Width = 420;
        window.Height = 860;
        window.MinimumWidth = 360;
        window.MinimumHeight = 640;
#endif

        return window;
    }
}
