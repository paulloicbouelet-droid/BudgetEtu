using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace BudgetEtu.Services;

/// <summary>
/// Adresse du serveur BudgetEtu.Api.
///  - PC Windows : http://localhost:5080
///  - Émulateur Android : http://10.0.2.2:5080 (10.0.2.2 = le PC vu depuis l'émulateur)
///  - Vrai téléphone : mettre l'adresse IP du PC (ex. http://192.168.1.20:5080) dans <see cref="BaseUrl"/>.
/// </summary>
public static class ApiSettings
{
    private const string Key = "api_base_url";

    public static string DefaultBaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android ? "http://10.0.2.2:5080" : "http://localhost:5080";

    public static string BaseUrl
    {
        get
        {
            try { return Preferences.Get(Key, DefaultBaseUrl); }
            catch { return DefaultBaseUrl; }
        }
        set
        {
            try { Preferences.Set(Key, value); }
            catch { /* préférences indisponibles : on garde l'adresse par défaut */ }
        }
    }
}
