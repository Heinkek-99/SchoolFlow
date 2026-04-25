using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Evaluations.Handlers;

namespace SchoolFlow.Api.Controllers;

[Route("api/evaluations")]
[Authorize(Policy = "TenantAccess")]
public class EvaluationsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? classeId, [FromQuery] Guid? periodeId,
        [FromQuery] Guid? anneeScolaireId, CancellationToken ct)
    {
        var result = await Mediator.Send(
            new GetAllEvaluationsQuery(classeId, periodeId, anneeScolaireId), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEvaluationCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Data });
    }

    [HttpGet("{id:guid}/notes")]
    public async Task<IActionResult> GetNotes(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetNotesEvaluationQuery(id), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<IActionResult> SaisirNotes(Guid id, [FromBody] SaisirNotesCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { EvaluationId = id }, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(new { notesSaisies = result.Data });
    }

    [HttpPut("{id:guid}/publier")]
    public async Task<IActionResult> Publier(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new PublierEvaluationCommand(id), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
