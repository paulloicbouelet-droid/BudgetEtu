using BudgetEtu.Services;
using BudgetEtu.ViewModels;
using BudgetEtu.Views;

namespace BudgetEtu;

public static class MauiProgram
{
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

        // Services (données fictives en attendant le backend)
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IBudgetService, BudgetService>();

        // ViewModels 
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<ForgotPasswordViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<BudgetsViewModel>();
        builder.Services.AddTransient<AlertsViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();
        builder.Services.AddTransient<TransactionViewModel>();

        // Pages
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ForgotPasswordPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<BudgetsPage>();
        builder.Services.AddTransient<AlertsPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<TransactionPage>();

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
        static void Flatten(Microsoft.UI.Xaml.Controls.Control control, params string[] prefixes)
        {
            var transparent = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
            control.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);

            foreach (var prefix in prefixes)
            foreach (var state in new[] { "", "PointerOver", "Pressed", "Focused", "Disabled" })
            {
                control.Resources[$"{prefix}Background{state}"] = transparent;
                control.Resources[$"{prefix}BorderBrush{state}"] = transparent;
            }

            control.Resources["TextControlBorderThemeThickness"] = new Microsoft.UI.Xaml.Thickness(0);
            control.Resources["TextControlBorderThemeThicknessFocused"] = new Microsoft.UI.Xaml.Thickness(0);
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
