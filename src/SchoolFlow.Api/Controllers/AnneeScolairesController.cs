using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.AnneeScolaires.Commands;
using SchoolFlow.Application.AnneeScolaires.Queries;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Api.Controllers;

[ApiController]
[Route("api/annees-scolaires")]
[Authorize(Policy = "TenantAccess")]
public class AnneeScolairesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllAnneesQuery(), ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateAnneeScolaireCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return StatusCode(201, new { id = result.Data });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAnneeScolaireRequest request, CancellationToken ct)
    {
        var command = new UpdateAnneeScolaireCommand(id, request.Libelle, request.DateDebut, request.DateFin);
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}/activer")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Activer(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActiverAnneeScolaireCommand(id), ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}/periodes")]
    public async Task<IActionResult> GetPeriodes(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPeriodesQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpPost("{id:guid}/periodes")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AddPeriode(Guid id, [FromBody] AddPeriodeRequest request, CancellationToken ct)
    {
        var command = new AddPeriodeCommand(
            id, request.Libelle, request.Type,
            request.DateDebut, request.DateFin, request.Numero);
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return StatusCode(201, new { id = result.Data });
    }
}

public record UpdateAnneeScolaireRequest(string Libelle, DateTime DateDebut, DateTime DateFin);

public record AddPeriodeRequest(
    string Libelle,
    TypePeriode Type,
    DateTime DateDebut,
    DateTime DateFin,
    int Numero
);
