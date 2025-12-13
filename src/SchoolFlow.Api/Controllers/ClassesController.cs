using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Classes.Queries;

namespace SchoolFlow.API.Controllers;

/// <summary>
/// Gestion des classes
/// </summary>
[Authorize]
public class ClassesController : BaseApiController
{
    /// <summary>
    /// Obtenir toutes les classes de l'année en cours
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await Mediator.Send(new GetAllClassesQuery());
        return Ok(result.Data);
    }

    /// <summary>
    /// Obtenir une classe par ID avec ses élèves
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetClasseByIdQuery(id));
        
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
        
        return Ok(result.Data);
    }
}