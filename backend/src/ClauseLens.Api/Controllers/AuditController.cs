using ClauseLens.Application.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClauseLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/audit-log")]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuditController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] Guid? contractId, [FromQuery] string? actionType,
                                          [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var r = await _mediator.Send(new ClauseLens.Application.Audit.Queries.GetAuditLogQuery(contractId, actionType, skip, take), ct);
        if (!r.IsSuccess) return BadRequest(new { error = r.Error, code = r.ErrorCode });
        return Ok(r.Value);
    }
}
