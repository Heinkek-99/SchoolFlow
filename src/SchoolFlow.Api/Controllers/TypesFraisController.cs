using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.TypesFrais.Commands;
using SchoolFlow.Application.TypesFrais.Queries;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Api.Controllers;

[ApiController]
[Route("api/types-frais")]
[Authorize(Policy = "TenantAccess")]
public class TypesFraisController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllTypesFraisQuery(), ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost]
    [Authorize(Policy = "ComptableAccess")]
    public async Task<IActionResult> Create([FromBody] CreateTypeFraisCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return StatusCode(201, new { id = result.Data });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ComptableAccess")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTypeFraisRequest request, CancellationToken ct)
    {
        var command = new UpdateTypeFraisCommand(
            id, request.Libelle, request.Description,
            request.IsObligatoire, request.GenerationAutomatique, request.MontantsParNiveau);
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ArchiveTypeFraisCommand(id), ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }
}

public record UpdateTypeFraisRequest(
    string Libelle,
    string? Description,
    bool IsObligatoire,
    bool GenerationAutomatique,
    Dictionary<Niveau, decimal> MontantsParNiveau
);
