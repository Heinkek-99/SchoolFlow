using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Dashboard.Queries;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.API.Controllers;

/// <summary>
/// Tableaux de bord et statistiques
/// </summary>
[Authorize]
public class DashboardController : BaseApiController
{
    /// <summary>
    /// Obtenir les statistiques principales
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStats), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var userId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var role = User.FindFirst("role")?.Value ?? "Secretaire";
        
        var result = await Mediator.Send(new GetDashboardStatsQuery(userId, role));
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtenir la liste des familles en impayé
    /// </summary>
    [HttpGet("impayes")]
    [Authorize(Policy = "ComptableAccess")]
    public async Task<IActionResult> GetImpayes()
    {
        var result = await Mediator.Send(new GetFamillesImpayesQuery());
        return Ok(result.Data);
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