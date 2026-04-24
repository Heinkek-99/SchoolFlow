// ============================================================
// FICHIER : src/SchoolFlow.Api/Controllers/EcolesController.cs
// ACTION  : Fichier à CRÉER
// ============================================================

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.API.Controllers;
using SchoolFlow.Application.Ecoles.Commands;
using SchoolFlow.Application.Ecoles.Queries;

namespace SchoolFlow.Api.Controllers;

/// <summary>
/// Gestion des établissements scolaires.
///
/// Endpoints publics (pas de token) :
///   POST /api/ecoles/inscription → Crée une école + Admin
///
/// Endpoints Admin (token requis, rôle Admin) :
///   PUT  /api/ecoles/{id}/infos
///   POST /api/ecoles/{id}/logo
///   GET  /api/ecoles/{id}
///
/// Endpoints SuperAdmin uniquement :
///   GET  /api/ecoles                → Liste toutes les écoles
///   PUT  /api/ecoles/{id}/valider   → Active une école
///   PUT  /api/ecoles/{id}/rejeter   → Rejette une demande
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EcolesController : BaseApiController
{

    // ─── INSCRIPTION PUBLIQUE ─────────────────────────────────────────────────

    /// <summary>
    /// Inscrit un nouvel établissement scolaire.
    /// Accès public — aucun token requis.
    /// L'école est créée en statut "En attente" jusqu'à validation Admin.
    /// </summary>
    [HttpPost("inscription")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Inscrire(
        [FromBody] CreerEcoleCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Data!.EcoleId },
            result.Data);
    }

    // ─── ADMIN : INFOS + LOGO ────────────────────────────────────────────────

    /// <summary>
    /// Détails de l'école connectée.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetEcoleByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Met à jour les infos générales de l'école (slogan, site web, email, téléphone secondaire).
    /// </summary>
    [HttpPut("{id:guid}/infos")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MettreAJour(
        Guid id,
        [FromBody] MettreAJourEcoleRequest request,
        CancellationToken ct)
    {
        var command = new MettreAJourEcoleCommand(
            id, request.Slogan, request.SiteWeb,
            request.TelephoneSecondaire, request.Email);

        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Upload du logo de l'établissement.
    /// </summary>
    [HttpPost("{id:guid}/logo")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UploadLogo(
        Guid id,
        IFormFile logo,
        [FromServices] SchoolFlow.Application.Common.Interfaces.IFileStorageService fileStorage,
        CancellationToken ct)
    {
        if (logo is null || logo.Length == 0)
            return BadRequest(new { error = "Fichier logo manquant." });

        var allowedTypes = new[] { "image/png", "image/jpeg", "image/webp" };
        if (!allowedTypes.Contains(logo.ContentType.ToLower()))
            return BadRequest(new { error = "Format accepté : PNG, JPEG, WebP." });

        if (logo.Length > 2 * 1024 * 1024) // 2 MB max
            return BadRequest(new { error = "Le logo ne peut pas dépasser 2 MB." });

        await using var stream = logo.OpenReadStream();
        var logoPath = await fileStorage.SaveFileAsync(stream, logo.FileName, "logos");

        var result = await Mediator.Send(new UploadLogoEcoleCommand(id, logoPath), ct);
        return result.IsSuccess
            ? Ok(new { message = result.Data, logoPath })
            : BadRequest(new { error = result.Error });
    }

    // ─── SUPER ADMIN : GESTION GLOBALE ───────────────────────────────────────

    /// <summary>
    /// Liste toutes les écoles (SuperAdmin uniquement).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? statut,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetAllEcolesQuery(search, statut, page, pageSize), ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Valide et active une école en attente (SuperAdmin uniquement).
    /// </summary>
    [HttpPut("{id:guid}/valider")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Valider(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ValiderEcoleCommand(id), ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Rejette une demande d'inscription d'école (SuperAdmin uniquement).
    /// </summary>
    [HttpPut("{id:guid}/rejeter")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Rejeter(
        Guid id,
        [FromBody] RejeterEcoleRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new RejeterEcoleCommand(id, request.Motif), ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }
}

// ─── REQUEST MODELS ──────────────────────────────────────────────────────────

public record MettreAJourEcoleRequest(
    string? Slogan,
    string? SiteWeb,
    string? TelephoneSecondaire,
    string? Email
);

public record RejeterEcoleRequest(string Motif);