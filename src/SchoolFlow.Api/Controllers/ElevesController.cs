using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Common.Interfaces;
using SchoolFlow.Application.Eleves.Commands;
using SchoolFlow.Application.Eleves.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.API.Controllers;

/// <summary>
/// Gestion des élèves
/// </summary>
[Authorize]
public class ElevesController : BaseApiController
{
    private readonly IFileStorageService _fileStorage;

    private static readonly string[] AllowedPhotoTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxPhotoSize = 5 * 1024 * 1024; // 5 Mo

    public ElevesController(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>
    /// Obtenir tous les élèves actifs
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EleveDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllElevesQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtenir un élève par ID avec son dossier complet
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EleveDossierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetElevesDossierQuery(id));

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Inscrire un nouvel élève.
    /// Accepte multipart/form-data (avec ou sans photo).
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "SecretaireAccess")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(CreateEleveResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateEleveCommand command, IFormFile? photo)
    {
        // Traitement de la photo si fournie
        string? photoPath = null;
        if (photo is { Length: > 0 })
        {
            var validationError = ValidatePhoto(photo);
            if (validationError != null)
                return BadRequest(new { error = validationError });

            await using var stream = photo.OpenReadStream();
            photoPath = await _fileStorage.SaveFileAsync(stream, photo.FileName, photo.ContentType);
        }

        // Injecter le photoPath dans la command (records sont immutables → with expression)
        var commandWithPhoto = command with { PhotoPath = photoPath };

        var result = await Mediator.Send(commandWithPhoto);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.EleveId }, result.Data);
    }

    /// <summary>
    /// Mettre à jour un élève.
    /// Accepte multipart/form-data (avec ou sans photo).
    /// Si aucune photo n'est fournie, l'ancienne est conservée.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SecretaireAccess")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateEleveCommand command, IFormFile? photo)
    {
        if (id != command.Id)
            return BadRequest(new { error = "ID mismatch" });

        // Traitement de la photo si une nouvelle est fournie
        string? photoPath = null;
        if (photo is { Length: > 0 })
        {
            var validationError = ValidatePhoto(photo);
            if (validationError != null)
                return BadRequest(new { error = validationError });

            await using var stream = photo.OpenReadStream();
            photoPath = await _fileStorage.SaveFileAsync(stream, photo.FileName, photo.ContentType);
        }

        // photoPath == null → le handler garde l'ancienne photo (null-guard dans UpdateEleveCommandHandler)
        var commandWithPhoto = command with { PhotoPath = photoPath };

        var result = await Mediator.Send(commandWithPhoto);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Archiver un élève (soft delete)
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    [Authorize(Policy = "DirecteurOrAdmin")]
    public async Task<IActionResult> Archive(Guid id, [FromBody] ArchiveEleveCommand command)
    {
        if (id != command.EleveId)
            return BadRequest(new { error = "ID mismatch" });

        var result = await Mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Obtenir les élèves d'une classe
    /// </summary>
    [HttpGet("classe/{classeId:guid}")]
    public async Task<IActionResult> GetByClasse(Guid classeId)
    {
        var result = await Mediator.Send(new GetElevesByClasseQuery(classeId));
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtenir les élèves d'une famille
    /// </summary>
    [HttpGet("famille/{familleId:guid}")]
    public async Task<IActionResult> GetByFamille(Guid familleId)
    {
        var result = await Mediator.Send(new GetElevesByFamilleQuery(familleId));
        return Ok(result.Data);
    }

    // ─────────────────────────────────────────
    // Helpers privés
    // ─────────────────────────────────────────

    private static string? ValidatePhoto(IFormFile photo)
    {
        if (!AllowedPhotoTypes.Contains(photo.ContentType))
            return "Format non supporté. Utilisez JPG, PNG ou WebP.";

        if (photo.Length > MaxPhotoSize)
            return "La photo ne doit pas dépasser 5 Mo.";

        return null;
    }
}