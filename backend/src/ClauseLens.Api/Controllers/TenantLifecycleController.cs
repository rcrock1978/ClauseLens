using ClauseLens.Application.Identity.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClauseLens.Api.Controllers;

/// <summary>
/// Per FR-013: tenant offboarding and GDPR right-to-erasure.
/// </summary>
[ApiController]
[Route("api/v1")]
public class TenantLifecycleController : ControllerBase
{
    private readonly IMediator _mediator;
    public TenantLifecycleController(IMediator mediator) => _mediator = mediator;

    [HttpPost("tenants/{tenantId:guid}/offboard")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Offboard(Guid tenantId, CancellationToken ct)
    {
        var r = await _mediator.Send(new OffboardTenantCommand(tenantId), ct);
        if (!r.IsSuccess) return BadRequest(new { error = r.Error, code = r.ErrorCode });
        return Accepted(r.Value);
    }

    [HttpPost("gdpr/erasure")]
    public async Task<IActionResult> Erasure([FromBody] ErasureRequest body, CancellationToken ct)
    {
        if (body is null) return BadRequest(new { error = "body required" });
        var r = await _mediator.Send(new RequestErasureCommand(body.SubjectEmail, body.Scope), ct);
        if (!r.IsSuccess) return BadRequest(new { error = r.Error, code = r.ErrorCode });
        return Accepted(r.Value);
    }
}

public sealed record ErasureRequest(string SubjectEmail, string Scope);
