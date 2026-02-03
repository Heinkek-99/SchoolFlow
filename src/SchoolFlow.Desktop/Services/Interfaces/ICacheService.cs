namespace SchoolFlow.Desktop.Services.Interfaces;

/// <summary>
/// Interface pour le cache en mémoire.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Récupère ou ajoute une valeur au cache.
    /// </summary>
    Task<T?> GetOrAddAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// Récupère une valeur du cache.
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// Ajoute ou met à jour une valeur dans le cache.
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Supprime une entrée du cache.
    /// </summary>
    void Remove(string key);

    /// <summary>
    /// Vide tout le cache.
    /// </summary>
    void Clear();

    /// <summary>
    /// Invalide les entrées commençant par un préfixe.
    /// </summary>
    void InvalidateByPrefix(string prefix);
}
