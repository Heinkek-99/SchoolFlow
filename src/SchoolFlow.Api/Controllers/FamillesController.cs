using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Familles.Commands;
using SchoolFlow.Application.Familles.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Api.Controllers;

/// <summary>
/// Gestion des familles
/// </summary>
[Authorize(Policy = "TenantAccess")]
public class FamillesController : BaseApiController
{
    /// <summary>
    /// Obtenir toutes les familles
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<FamilleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllFamillesQuery query)
    {
        var result = await Mediator.Send(query);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtenir une famille par ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FamilleDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetFamilleByIdQuery(id));
        
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Créer une nouvelle famille
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "SecretaireAccess")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateFamilleCommand command)
    {
        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
    }

    /// <summary>
    /// Mettre à jour une famille
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "SecretaireAccess")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFamilleCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { error = "ID mismatch" });
        
        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        
        return NoContent();
    }

    /// <summary>
    /// Archiver une famille (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Archive(Guid id)
    {
        var result = await Mediator.Send(new ArchiveFamilleCommand(id));
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        
        return NoContent();
    }

    /// <summary>
    /// Rechercher familles par nom/téléphone
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var result = await Mediator.Send(new SearchFamillesQuery(query));
        return Ok(result.Data);
    }
}