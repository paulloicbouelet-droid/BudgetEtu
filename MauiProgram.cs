using BudgetEtu.Services;
using BudgetEtu.ViewModels;
using BudgetEtu.Views;

namespace BudgetEtu;

public static class MauiProgram
{
    // Champ (et non constante) pour éviter l'avertissement « code inaccessible » quand on change la valeur.
    private static readonly bool UseRealApi = true;

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Police unique de l'application (cf. Resources/Styles/Styles.xaml)
                fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
                fonts.AddFont("Poppins-Medium.ttf", "PoppinsMedium");
                fonts.AddFont("Poppins-Bold.ttf", "PoppinsBold");
            });

        ConfigureHandlers();

        // ----- Services -----
        // true  : l'application parle au serveur BudgetEtu.Api (à lancer avant : voir README).
        // false : services fictifs en mémoire (AuthService / BudgetService), sans serveur : pratique pour une démo hors ligne.
        if (UseRealApi)
        {
            builder.Services.AddSingleton(new HttpClient { Timeout = TimeSpan.FromSeconds(10) });
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<IAuthService, ApiAuthService>();
            builder.Services.AddSingleton<IBudgetService, ApiBudgetService>();
        }
        else
        {
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<IBudgetService, BudgetService>();
        }

        // ViewModels 
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<BudgetsViewModel>();
        builder.Services.AddTransient<AlertsViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<TransactionViewModel>();
        builder.Services.AddTransient<BudgetFormViewModel>();
        builder.Services.AddTransient<ChangePasswordViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<BudgetsPage>();
        builder.Services.AddTransient<AlertsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<TransactionPage>();
        builder.Services.AddTransient<BudgetFormPage>();
        builder.Services.AddTransient<ChangePasswordPage>();

        return builder.Build();
    }

    /// <summary>
    /// Retire le soulignement natif des champs : le cadre arrondi (Border) fait office de contour,
    /// comme dans les maquettes.
    /// </summary>
    private static void ConfigureHandlers()
    {
#if ANDROID
        static void Clear(Android.Views.View? view)
        {
            if (view is not null)
                view.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
        }

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) => Clear(handler.PlatformView));
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) => Clear(handler.PlatformView));
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, _) => Clear(handler.PlatformView));
#elif WINDOWS
        // WinUI dessine sa propre bordure/fond sur les champs : on les rend transparents,
        // le cadre arrondi (Border) des maquettes les remplace.
        // Purement cosmétique : toute erreur ici est ignorée, pour ne jamais empêcher l'ouverture d'un écran.
        static void Flatten(Microsoft.UI.Xaml.Controls.Control control, params string[] prefixes)
        {
            static void TrySet(Microsoft.UI.Xaml.ResourceDictionary resources, string key, object value)
            {
                try { resources[key] = value; }
                catch { /* clé refusée par ce contrôle : on l'ignore */ }
            }

            static Microsoft.UI.Xaml.Media.SolidColorBrush NewTransparent()
                => new(Microsoft.UI.Colors.Transparent);

            try { control.BorderThickness = new Microsoft.UI.Xaml.Thickness(0); }
            catch { /* ignoré */ }

            foreach (var prefix in prefixes)
            foreach (var state in new[] { "", "PointerOver", "Pressed", "Focused", "Disabled" })
            {
                // Un pinceau neuf par clé : on ne partage pas le même objet entre plusieurs entrées.
                TrySet(control.Resources, $"{prefix}Background{state}", NewTransparent());
                TrySet(control.Resources, $"{prefix}BorderBrush{state}", NewTransparent());
            }

            TrySet(control.Resources, "TextControlBorderThemeThickness", new Microsoft.UI.Xaml.Thickness(0));
            TrySet(control.Resources, "TextControlBorderThemeThicknessFocused", new Microsoft.UI.Xaml.Thickness(0));
        }

        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("Flat", (handler, _) => Flatten(handler.PlatformView, "TextControl"));
        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("Flat", (handler, _) => Flatten(handler.PlatformView, "ComboBox"));
        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("Flat", (handler, _) => Flatten(handler.PlatformView, "CalendarDatePicker"));
#elif IOS
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, _) =>
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None);
#endif
    }
}
