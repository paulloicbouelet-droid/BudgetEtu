namespace BudgetEtu.Controls;

/// <summary>Accès aux couleurs et polices de AppStyles.xaml depuis le code.</summary>
internal static class ResourceHelper
{
    public static Color Color(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Color color)
            return color;

        return Colors.Gray;
    }

    /// <summary>Alias de police défini dans AppStyles.xaml (FontTitle, FontBody, FontMedium, FontBold).</summary>
    public static string Font(string key)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) == true && value is string font)
            return font;

        return string.Empty;
    }
}
