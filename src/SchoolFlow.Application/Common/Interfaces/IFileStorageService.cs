namespace SchoolFlow.Application.Common.Interfaces;

/// <summary>
/// Abstraction pour la sauvegarde de fichiers uploadés.
/// L'implémentation concrète est dans Infrastructure.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Sauvegarde un fichier et retourne son URL absolue stockée en BDD (PhotoPath).
    /// </summary>
    Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken ct = default);

    /// <summary>
    /// Supprime un fichier à partir de son URL/path stocké en BDD.
    /// </summary>
    Task DeleteFileAsync(string fileUrl, CancellationToken ct = default);
}