using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Disciplines.Handlers;

namespace SchoolFlow.Api.Controllers;

[Route("api/disciplines")]
[Authorize(Policy = "TenantAccess")]
public class DisciplinesController : BaseApiController
{
    [HttpGet("eleve/{eleveId:guid}")]
    public async Task<IActionResult> GetParEleve(Guid eleveId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetDisciplinesEleveQuery(eleveId), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDisciplineCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(new { id = result.Data });
    }
}
