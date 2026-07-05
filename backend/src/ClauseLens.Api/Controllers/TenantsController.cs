using ClauseLens.Application.Identity.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClauseLens.Api.Controllers;

/// <summary>
/// Per FR-007c — self-service tenant signup and email verification.
/// </summary>
[ApiController]
[Route("api/v1")]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;
    public TenantsController(IMediator mediator) => _mediator = mediator;

    [HttpPost("tenants/signup")]
    public async Task<IActionResult> Signup([FromBody] SignupTenantRequest body, CancellationToken ct)
    {
        if (body is null) return BadRequest(new { error = "body required" });
        var result = await _mediator.Send(new SignupTenantCommand(body.TenantName, body.AdminEmail, body.Password), ct);
        if (!result.IsSuccess) return Conflict(new { error = result.Error, code = result.ErrorCode });
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}

public sealed record SignupTenantRequest(string TenantName, string AdminEmail, string Password);
