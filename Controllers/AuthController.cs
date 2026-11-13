using System.Security.Claims;
using FinData.Application.Commands.LoginUser;
using FinData.Application.Commands.RegisterUser;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinData.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        [FromServices] RegisterUserHandler handler)
    {
        var (result, userId) = await handler.HandleAsync(command);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(error => error.Description)
            });
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new { id = userId });
    }

    [HttpPost("login")]
    [ProducesResponseType(
        typeof(AccessTokenResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        [FromServices] LoginUserHandler handler)
    {
        var result = await handler.HandleAsync(command);

        if (!result.Succeeded)
        {
            return Unauthorized(new
            {
                message = "E-mail ou senha inválidos."
            });
        }

        // O Identity já escreveu os tokens no corpo da resposta.
        return new EmptyResult();
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var id))
            return Unauthorized();

        return Ok(new { id });
    }
}