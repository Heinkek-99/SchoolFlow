using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    /// Inscrire un nouvel élève
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "SecretaireAccess")]
    [ProducesResponseType(typeof(CreateEleveResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateEleveCommand command)
    {
        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.EleveId }, result.Data);
    }

    /// <summary>
    /// Mettre à jour un élève
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SecretaireAccess")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEleveCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { error = "ID mismatch" });
        
        var result = await Mediator.Send(command);
        
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
}