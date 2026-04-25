using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Examens.Handlers;

namespace SchoolFlow.Api.Controllers;

[Route("api/examens")]
[Authorize(Policy = "TenantAccess")]
public class ExamensController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? anneeScolaireId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllExamensQuery(anneeScolaireId), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet("{id:guid}/resultats")]
    public async Task<IActionResult> GetResultats(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetResultatsExamenQuery(id), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExamenCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Data });
    }

    [HttpPost("{id:guid}/inscrire")]
    public async Task<IActionResult> Inscrire(Guid id, [FromBody] InscrireEleveExamenCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { ExamenId = id }, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(new { inscrits = result.Data });
    }

    [HttpPut("inscriptions/{inscriptionId:guid}/resultats")]
    public async Task<IActionResult> SaisirResultat(
        Guid inscriptionId, [FromBody] SaisirResultatExamenCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { InscriptionId = inscriptionId }, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
