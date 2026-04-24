using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.API.Controllers;
using SchoolFlow.Application.Utilisateurs.Commands;
using SchoolFlow.Application.Utilisateurs.Queries;
using SchoolFlow.Domain.Entities;

namespace SchoolFlow.Api.Controllers;

[Authorize]
public class UtilisateursController : BaseApiController
{
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAll([FromQuery] string? search, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetAllUtilisateursQuery(search), ct);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.Error });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetUtilisateurByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result.Data) : NotFound(new { error = result.Error });
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateUtilisateurCommand command, CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error });
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, new { id = result.Data });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUtilisateurRequest request, CancellationToken ct)
    {
        var command = new UpdateUtilisateurCommand(id, request.Nom, request.Prenom, request.Email, request.Telephone);
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }

    [HttpPut("{id:guid}/password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var command = new ChangePasswordCommand(id, request.AncienMotDePasse, request.NouveauMotDePasse);
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ArchiverUtilisateurCommand(id), ct);
        return result.IsSuccess ? Ok(new { message = result.Data }) : BadRequest(new { error = result.Error });
    }
}

public record UpdateUtilisateurRequest(string Nom, string Prenom, string? Email, string? Telephone);
public record ChangePasswordRequest(string AncienMotDePasse, string NouveauMotDePasse);
