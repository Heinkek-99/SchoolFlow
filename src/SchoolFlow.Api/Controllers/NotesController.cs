using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Evaluations.Handlers;

namespace SchoolFlow.Api.Controllers;

[Route("api/notes")]
[Authorize(Policy = "TenantAccess")]
public class NotesController : BaseApiController
{
    [HttpGet("eleve/{eleveId:guid}")]
    public async Task<IActionResult> GetNotesEleve(
        Guid eleveId, [FromQuery] Guid? periodeId, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetNotesEleveQuery(eleveId, periodeId), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return Ok(result.Data);
    }
}
