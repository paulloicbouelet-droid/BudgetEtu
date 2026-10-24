using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Networking;

namespace BudgetEtu.Services;

/// <summary>
/// Le seul endroit qui parle HTTP avec le serveur BudgetEtu.Api.
/// - ajoute le jeton JWT (« Authorization: Bearer … ») aux requêtes qui le demandent ;
/// - transforme les erreurs HTTP en <see cref="AppException"/> : les écrans affichent déjà la bonne réaction
///   (hors ligne, serveur en panne, session expirée, message de validation).
/// </summary>
public class ApiClient
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    /// <summary>Jeton de la session en cours (gardé en mémoire : il faut se reconnecter à chaque lancement).</summary>
    public string? Token { get; set; }

    public async Task<T> GetAsync<T>(string path)
    {
        using var response = await SendAsync(HttpMethod.Get, path, body: null, anonymous: false);
        return await ReadAsync<T>(response);
    }

    /// <summary>POST avec réponse JSON. <paramref name="anonymous"/> = route publique (connexion, inscription…).</summary>
    public async Task<T> PostAsync<T>(string path, object body, bool anonymous = false)
    {
        using var response = await SendAsync(HttpMethod.Post, path, body, anonymous);
        return await ReadAsync<T>(response);
    }

    /// <summary>POST sans réponse attendue (204).</summary>
    public async Task PostAsync(string path, object body, bool anonymous = false)
    {
        using var response = await SendAsync(HttpMethod.Post, path, body, anonymous);
    }

    /// <summary>PUT sans réponse attendue (204) : modification d'un élément existant.</summary>
    public async Task PutAsync(string path, object body)
    {
        using var response = await SendAsync(HttpMethod.Put, path, body, anonymous: false);
    }

    /// <summary>DELETE sans réponse attendue (204).</summary>
    public async Task DeleteAsync(string path)
    {
        using var response = await SendAsync(HttpMethod.Delete, path, body: null, anonymous: false);
    }

    // ----- Interne -----

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, bool anonymous)
    {
        // Menu Démo : permet encore de simuler hors ligne / erreur serveur / session expirée / aucune donnée.
        // En fonctionnement normal, ne coûte rien.
        await Simulation.EnterAsync(requiresSession: !anonymous);

        var url = $"{ApiSettings.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        using var request = new HttpRequestMessage(method, url);

        if (body is not null)
            request.Content = JsonContent.Create(body, body.GetType(), options: Json);

        if (!anonymous && !string.IsNullOrEmpty(Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (TaskCanceledException)
        {
            // Délai dépassé : le serveur ne répond pas.
            throw new AppException(ErrorKind.Server, "Le serveur ne répond pas.");
        }
        catch (HttpRequestException)
        {
            // Connexion impossible : pas de réseau sur l'appareil, ou serveur arrêté.
            throw new AppException(
                HasNetwork() ? ErrorKind.Server : ErrorKind.Offline,
                "Impossible de joindre le serveur.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await ToExceptionAsync(response, anonymous);
            response.Dispose();
            throw error;
        }

        return response;
    }

    private async Task<AppException> ToExceptionAsync(HttpResponseMessage response, bool anonymous)
    {
        var code = (int)response.StatusCode;

        // 401 sur une route protégée : le jeton a expiré (ou n'existe plus).
        if (code == 401 && !anonymous)
        {
            Token = null;
            return new AppException(ErrorKind.SessionExpired, "Ta session a expiré.");
        }

        if (code >= 500)
            return new AppException(ErrorKind.Server, "Le serveur a rencontré une erreur.");

        // 400, 401 (identifiants), 404, 409… : le serveur explique dans « detail » ce qui ne va pas.
        var detail = await ReadDetailAsync(response);
        return new AppException(ErrorKind.Validation, detail ?? "La demande n'a pas pu être traitée.");
    }

    private static async Task<string?> ReadDetailAsync(HttpResponseMessage response)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ApiProblem>(Json);
            return problem?.Detail ?? problem?.Title;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>(Json)
                   ?? throw new AppException(ErrorKind.Server, "Réponse du serveur vide.");
        }
        catch (JsonException)
        {
            throw new AppException(ErrorKind.Server, "Réponse du serveur illisible.");
        }
    }

    private static bool HasNetwork()
    {
        try
        {
            return Connectivity.Current.NetworkAccess != NetworkAccess.None;
        }
        catch
        {
            return true;
        }
    }
}
