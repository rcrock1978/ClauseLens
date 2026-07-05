using ClauseLens.Application.Analysis.Commands;
using ClauseLens.Application.Analysis.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClauseLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/contracts/{contractId:guid}")]
public class AnalysisController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnalysisController(IMediator mediator) => _mediator = mediator;

    [HttpPost("analyze")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("per-tenant-upload")]
    public async Task<IActionResult> Analyze(Guid contractId, CancellationToken ct)
    {
        var r = await _mediator.Send(new AnalyzeContractCommand(contractId), ct);
        if (!r.IsSuccess) return BadRequest(new { error = r.Error, code = r.ErrorCode });
        return Accepted(r.Value);
    }

    [HttpGet("risks")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("per-tenant-read")]
    public async Task<IActionResult> Risks(Guid contractId, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetRisksQuery(contractId), ct);
        if (!r.IsSuccess) return NotFound(new { error = r.Error, code = r.ErrorCode });
        return Ok(r.Value);
    }

    [HttpGet("redlines")]
    public async Task<IActionResult> Redlines(Guid contractId, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetRedlinesQuery(contractId), ct);
        if (!r.IsSuccess) return NotFound(new { error = r.Error, code = r.ErrorCode });
        return Ok(r.Value);
    }

    [HttpGet("obligations")]
    public async Task<IActionResult> Obligations(Guid contractId, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetObligationsQuery(contractId), ct);
        if (!r.IsSuccess) return NotFound(new { error = r.Error, code = r.ErrorCode });
        return Ok(r.Value);
    }

    [HttpGet("clauses/{clauseId:guid}/comparison")]
    public async Task<IActionResult> Compare(Guid contractId, Guid clauseId, CancellationToken ct)
    {
        var r = await _mediator.Send(new GetClauseComparisonQuery(contractId, clauseId), ct);
        if (!r.IsSuccess) return NotFound(new { error = r.Error, code = r.ErrorCode });
        return Ok(r.Value);
    }
}
