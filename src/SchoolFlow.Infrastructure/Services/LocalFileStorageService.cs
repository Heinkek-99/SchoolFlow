using Microsoft.Extensions.Configuration;
using SchoolFlow.Application.Common.Interfaces;

namespace SchoolFlow.Infrastructure.Services;

/// <summary>
/// Stockage local des fichiers uploadés.
/// Utilise la configuration (ApplicationSettings) pour éviter toute dépendance
/// sur Microsoft.AspNetCore.* — compatible class library Infrastructure.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;
    private readonly string _baseUrl;
    private const string PhotosSubFolder = "uploads/photos";

    public LocalFileStorageService(IConfiguration configuration)
    {
        // Base URL de l'API, ex: "https://schoolflow-8e86.onrender.com"
        // Configurée dans appsettings.json > ApplicationSettings > BaseUrl
        _baseUrl = configuration["ApplicationSettings:BaseUrl"]
                   ?? "http://localhost:5000";

        // Chemin physique racine du serveur, ex: "wwwroot" ou chemin absolu
        // Configurée dans appsettings.json > ApplicationSettings > WebRootPath
        _webRootPath = configuration["ApplicationSettings:WebRootPath"]
                       ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        // Dossier physique : wwwroot/uploads/photos/
        var uploadPath = Path.Combine(_webRootPath, "uploads", "photos");
        Directory.CreateDirectory(uploadPath);

        // Nom unique pour éviter les collisions
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadPath, uniqueName);

        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await fileStream.CopyToAsync(fs, ct);

        // URL absolue stockée en BDD → frontend peut charger directement
        return $"{_baseUrl.TrimEnd('/')}/{PhotosSubFolder}/{uniqueName}";
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        try
        {
            // Extraire le nom de fichier depuis l'URL
            var uri = new Uri(fileUrl);
            var fileName = Path.GetFileName(uri.LocalPath);
            var fullPath = Path.Combine(_webRootPath, "uploads", "photos", fileName);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch
        {
            // Ne pas faire planter l'appel si suppression impossible
        }

        return Task.CompletedTask;
    }
}