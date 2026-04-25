using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Dashboard.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Api.Controllers;

/// <summary>
/// Tableaux de bord et statistiques
/// </summary>
[Authorize(Policy = "TenantAccess")]
public class DashboardController : BaseApiController
{

    /// <summary>
    /// Obtenir les statistiques principales
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var role = User.FindFirst("role")?.Value ?? "Secretaire";
        
        var result = await Mediator.Send(new GetDashboardStatsQuery());
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Obtenir la liste des familles en impayé
    /// </summary>
    [HttpGet("impayes")]
    [Authorize(Policy = "ComptableAccess")]
    [ProducesResponseType(typeof(List<FamilleImpayeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetImpayes(
        [FromQuery] int? limite = null,
        [FromQuery] int? joursRetardMin = null)
    {
        var query = new GetFamillesImpayesQuery(limite, joursRetardMin);
        var result = await Mediator.Send(query);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

     /// <summary>
    /// Récupère le top 10 des familles les plus endettées
    /// </summary>
    [HttpGet("top-impayes")]
    [Authorize(Policy = "ComptableAccess")]
    [ProducesResponseType(typeof(List<FamilleImpayeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopImpayes()
    {
        var query = new GetFamillesImpayesQuery(LimiteResultats: 10);
        var result = await Mediator.Send(query);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.Error);
    }

    /// <summary>
    /// Obtenir les statistiques financières détaillées
    /// </summary>
    [HttpGet("finances")]
    [Authorize(Policy = "DirecteurOrAdmin")]
    public async Task<IActionResult> GetFinances([FromQuery] GetStatsFinancieresQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result.Data);
    }
}