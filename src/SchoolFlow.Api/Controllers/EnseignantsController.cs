using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Enseignants.Handlers;

namespace SchoolFlow.Api.Controllers;

[Route("api/enseignants")]
[Authorize(Policy = "TenantAccess")]
public class EnseignantsController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllEnseignantsQuery(), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetEnseignantByIdQuery(id), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEnseignantCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Data });
    }

    [HttpPost("{id:guid}/matieres")]
    public async Task<IActionResult> AssignerMatiere(Guid id, [FromBody] AssignerMatiereCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command with { EnseignantId = id }, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return NoContent();
    }
}
