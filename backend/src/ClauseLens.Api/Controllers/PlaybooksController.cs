using ClauseLens.Application.Playbooks.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClauseLens.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/playbooks")]
public class PlaybooksController : ControllerBase
{
    private readonly IMediator _mediator;
    public PlaybooksController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public IActionResult List() => Ok(Array.Empty<object>());

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] ImportRequest body, CancellationToken ct)
    {
        var r = await _mediator.Send(new ImportPlaybookTemplatesCommand(body.TemplateIds ?? new()), ct);
        if (!r.IsSuccess) return BadRequest(new { error = r.Error, code = r.ErrorCode });
        return CreatedAtAction(nameof(List), r.Value);
    }

    [HttpPost("rules/{ruleId:guid}/publish")]
    public async Task<IActionResult> Publish(Guid ruleId, CancellationToken ct)
    {
        var r = await _mediator.Send(new PublishPlaybookRuleCommand(ruleId), ct);
        if (!r.IsSuccess) return StatusCode(StatusCodes.Status403Forbidden, new { error = r.Error, code = r.ErrorCode });
        return Ok(r.Value);
    }
}

public sealed record ImportRequest(List<string>? TemplateIds);
