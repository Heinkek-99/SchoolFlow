namespace SchoolFlow.Desktop.Services.Interfaces;

/// <summary>
/// Interface pour les appels API REST.
/// </summary>
public interface IApiService
{
    /// <summary>
    /// Effectue une requête GET.
    /// </summary>
    Task<T?> GetAsync<T>(string endpoint);

    /// <summary>
    /// Effectue une requête POST.
    /// </summary>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data);

    /// <summary>
    /// Effectue une requête PUT.
    /// </summary>
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data);

    /// <summary>
    /// Effectue une requête DELETE.
    /// </summary>
    Task<bool> DeleteAsync(string endpoint);

    /// <summary>
    /// Définit le token JWT pour les requêtes authentifiées.
    /// </summary>
    void SetAuthToken(string token);

    /// <summary>
    /// Supprime le token d'authentification.
    /// </summary>
    void ClearAuthToken();
}
