using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Bulletins.Handlers;

namespace SchoolFlow.Api.Controllers;

[Route("api/bulletins")]
[Authorize(Policy = "TenantAccess")]
public class BulletinsController : BaseApiController
{
    [HttpPost("generer")]
    public async Task<IActionResult> Generer([FromBody] GenererBulletinsCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(new { bulletinsGeneres = result.Data });
    }

    [HttpGet("classe/{classeId:guid}")]
    public async Task<IActionResult> GetParClasse(
        Guid classeId, [FromQuery] Guid periodeId,
        [FromQuery] Guid anneeScolaireId, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetBulletinsClasseQuery(classeId, periodeId, anneeScolaireId), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet("eleve/{eleveId:guid}")]
    public async Task<IActionResult> GetParEleve(Guid eleveId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetBulletinsEleveQuery(eleveId), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetBulletinDetailQuery(id), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPut("{id:guid}/publier")]
    public async Task<IActionResult> Publier(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new PublierBulletinCommand(id), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpPut("{id:guid}/appreciation")]
    public async Task<IActionResult> AjouterAppréciation(
        Guid id, [FromBody] AjouterAppreciationDirecteurCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { Id = id }, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
