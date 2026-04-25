using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.EmploiDuTemps.Handlers;

namespace SchoolFlow.Api.Controllers;

[Route("api/emploi-du-temps")]
[Authorize(Policy = "TenantAccess")]
public class EmploiDuTempsController : BaseApiController
{
    [HttpGet("classe/{classeId:guid}")]
    public async Task<IActionResult> GetParClasse(
        Guid classeId, [FromQuery] Guid anneeScolaireId, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetEmploiDuTempsClasseQuery(classeId, anneeScolaireId), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCreneauCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Data });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new SupprimerCreneauCommand(id), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
