using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Classes.Commands;
using SchoolFlow.Application.Classes.Queries;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Api.Controllers;

/// <summary>
/// Gestion des classes
/// </summary>
[Authorize(Policy = "TenantAccess")]
public class ClassesController : BaseApiController
{
    /// <summary>
    /// Obtenir toutes les classes de l'année en cours
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllClassesQuery(), ct);
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtenir une classe par ID avec ses élèves
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetClasseByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : NotFound(new { error = result.Error });
    }

    /// <summary>
    /// Créer une nouvelle classe
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateClasseCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return StatusCode(201, new { id = result.Data });
    }

    /// <summary>
    /// Mettre à jour une classe
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClasseRequest request, CancellationToken ct)
    {
        var command = new UpdateClasseCommand(
            id, request.Nom, request.Niveau, request.SousSysteme,
            request.Section, request.CapaciteMax, request.TitulaireId);
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Archiver une classe (suppression logique)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ArchiveClasseCommand(id), ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }
}

public record UpdateClasseRequest(
    string Nom,
    Niveau Niveau,
    SousSysteme SousSysteme,
    string? Section,
    int CapaciteMax,
    Guid? TitulaireId
);