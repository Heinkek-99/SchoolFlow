using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolFlow.Application.Auth.Commands;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Api.Controllers;

/// <summary>
/// Gestion de l'authentification
/// </summary>
public class AuthController : BaseApiController
{
    /// <summary>
    /// Connexion utilisateur
    /// </summary>
    /// <param name="command">Identifiants de connexion</param>
    /// <returns>Token JWT et infos utilisateur</returns>
    /// <response code="200">Connexion réussie</response>
    /// <response code="400">Identifiants incorrects</response>
    /// <response code="401">Compte verrouillé</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await Mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        
        return Ok(result.Data);
    }

    /// <summary>
    /// Déconnexion utilisateur (invalidation token côté client)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        // En JWT, logout se fait côté client (supprimer token)
        return Ok(new { message = "Déconnexion réussie" });
    }

    /// <summary>
    /// Obtenir les informations de l'utilisateur connecté
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        var username = User.Identity?.Name;
        var role = User.FindFirst("role")?.Value;
        
        return Ok(new 
        { 
            userId, 
            username, 
            role,
            isAuthenticated = User.Identity?.IsAuthenticated ?? false
        });
    }
}