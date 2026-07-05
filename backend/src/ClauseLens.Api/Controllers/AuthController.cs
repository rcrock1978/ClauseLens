using ClauseLens.Application.Identity.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClauseLens.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest body, CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.Token))
            return BadRequest(new { error = "token required" });
        var result = await _mediator.Send(new VerifyEmailCommand(body.Token), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken ct)
    {
        if (body is null || string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password))
            return BadRequest(new { error = "email and password are required" });
        var result = await _mediator.Send(new LoginCommand(body.Email, body.Password), ct);
        if (!result.IsSuccess) return Unauthorized(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }
}

public sealed record VerifyEmailRequest(string Token);
public sealed record LoginRequest(string Email, string Password);
