using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClauseLens.Api.Controllers;

/// <summary>
/// Per FR-005, FR-007a, FR-007b, FR-007c: review workflow endpoints.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/contracts/{contractId:guid}/review")]
public class ReviewController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReviewController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> Assign(Guid contractId, [FromBody] AssignReviewRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new ClauseLens.Application.Review.Commands.AssignReviewCommand(
            contractId, body.PrimaryReviewerId, body.SecondaryReviewerIds ?? new()), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpPost("reassign")]
    public async Task<IActionResult> Reassign(Guid contractId, [FromBody] ReassignRequest body, CancellationToken ct)
    {
        var result = await _mediator.Send(new ClauseLens.Application.Review.Commands.ReassignReviewCommand(
            contractId, body.NewPrimaryReviewerId, body.Reason), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpPut("clauses/{clauseId:guid}/decision")]
    public async Task<IActionResult> Decide(Guid contractId, Guid clauseId, [FromBody] DecideRequest body, CancellationToken ct)
    {
        if (!Enum.TryParse<ClauseDecisionType>(body.Decision, true, out var decision))
            return BadRequest(new { error = "decision must be one of: Approved, RejectedWithComment, NeedsDiscussion", code = "INVALID_DECISION" });
        var result = await _mediator.Send(new ClauseLens.Application.Review.Commands.DecideClauseCommand(
            contractId, clauseId, decision, body.Comment), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit(Guid contractId, CancellationToken ct)
    {
        var result = await _mediator.Send(new ClauseLens.Application.Review.Commands.SubmitReviewCommand(contractId), ct);
        if (!result.IsSuccess) return BadRequest(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }

    [HttpPost("comments")]
    public async Task<IActionResult> AddComment(Guid contractId, [FromBody] CommentRequest body, CancellationToken ct)
    {
        if (body is null) return BadRequest(new { error = "body required" });
        var result = await _mediator.Send(new ClauseLens.Application.Review.Commands.AddSecondaryCommentCommand(contractId, body.ClauseId, body.Comment), ct);
        if (!result.IsSuccess) return StatusCode(StatusCodes.Status403Forbidden, new { error = result.Error, code = result.ErrorCode });
        return Created(result.Value);
    }
}

public sealed record AssignReviewRequest(Guid PrimaryReviewerId, List<Guid>? SecondaryReviewerIds);
public sealed record ReassignRequest(Guid NewPrimaryReviewerId, string Reason);
public sealed record DecideRequest(string Decision, string? Comment);
public sealed record CommentRequest(Guid ClauseId, string Comment);
