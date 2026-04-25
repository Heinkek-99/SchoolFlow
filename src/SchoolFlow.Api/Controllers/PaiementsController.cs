using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Paiements.Commands;
using SchoolFlow.Application.Paiements.Queries;

namespace SchoolFlow.Api.Controllers;

/// <summary>
/// Gestion des paiements
/// </summary>
[Authorize(Policy = "ComptableAccess")]
public class PaiementsController : BaseApiController
{
    /// <summary>
    /// Enregistrer un nouveau paiement avec ventilation
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] EnregistrerPaiementCommand command)
    {
        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error, errors = result.Errors });
        
        return CreatedAtAction(nameof(GetByNumero), new { numero = result.Data }, new { numeroPaiement = result.Data });
    }

    /// <summary>
    /// Obtenir un paiement par son numéro
    /// </summary>
    [HttpGet("{numero}")]
    public async Task<IActionResult> GetByNumero(string numero)
    {
        var result = await Mediator.Send(new GetPaiementByNumeroQuery(numero));
        
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtenir l'historique des paiements d'une famille
    /// </summary>
    [HttpGet("famille/{familleId:guid}")]
    public async Task<IActionResult> GetByFamille(Guid familleId)
    {
        var result = await Mediator.Send(new GetHistoriquePaiementsFamilleQuery(familleId));
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtenir tous les paiements (avec filtres)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllPaiementsQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result.Data);
    }

    /// <summary>
    /// Proposer une ventilation automatique pour un montant
    /// </summary>
    [HttpPost("ventilation/proposer")]
    public async Task<IActionResult> ProposerVentilation([FromBody] ProposerVentilationQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result.Data);
    }
}