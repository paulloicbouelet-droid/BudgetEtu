using System.Globalization;

namespace BudgetEtu.Services;

/// <summary>Formatage des montants en FCFA : « 150 000 FCFA » (espace insécable entre les milliers).</summary>
public static class Fcfa
{
    private static readonly NumberFormatInfo Format = new() { NumberGroupSeparator = " " };

    /// <summary>150 000</summary>
    public static string Number(decimal value) => value.ToString("N0", Format);

    /// <summary>150 000 FCFA</summary>
    public static string Amount(decimal value) => $"{Number(value)} FCFA";

    /// <summary>« + 200 000 FCFA » ou « - 50 000 FCFA »</summary>
    public static string Signed(decimal value, bool isExpense)
        => $"{(isExpense ? "-" : "+")} {Amount(Math.Abs(value))}";
}
