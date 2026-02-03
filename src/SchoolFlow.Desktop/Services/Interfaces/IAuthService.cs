using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Desktop.Services.Interfaces;

/// <summary>
/// Interface pour l'authentification.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Utilisateur actuellement connecté.
    /// </summary>
    LoginResponse? CurrentUser { get; }

    /// <summary>
    /// Indique si un utilisateur est authentifié.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Événement déclenché lors d'un changement d'état d'authentification.
    /// </summary>
    event Action<bool>? AuthStateChanged;

    /// <summary>
    /// Authentifie un utilisateur.
    /// </summary>
    Task<LoginResponse?> LoginAsync(string username, string password);

    /// <summary>
    /// Déconnecte l'utilisateur actuel.
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Vérifie si l'utilisateur a un rôle spécifique.
    /// </summary>
    bool HasRole(string role);
}
