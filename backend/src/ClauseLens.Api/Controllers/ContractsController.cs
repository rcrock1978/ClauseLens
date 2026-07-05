using ClauseLens.Application.Abstractions;
using ClauseLens.Application.Contracts.Commands;
using ClauseLens.Application.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClauseLens.Api.Controllers;

/// <summary>
/// Contract lifecycle endpoints. Per FR-001/FR-002/FR-002a:
///   - POST /api/v1/contracts  upload + segment
///   - GET  /api/v1/contracts/{id}  return detail with clause list
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/contracts")]
public class ContractsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEnumerable<IDocumentParser> _parsers;

    public ContractsController(IMediator mediator, IEnumerable<IDocumentParser> parsers)
    { _mediator = mediator; _parsers = parsers; }

    [HttpPost]
    [RequestSizeLimit(26 * 1024 * 1024)]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("per-tenant-upload")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null) return BadRequest(new { error = "file is required", code = "MISSING_FILE" });
        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var format = ext == "docx" ? "docx" : "pdf";
        var parser = _parsers.FirstOrDefault(p => p.SupportsFormat(format));
        if (parser is null) return BadRequest(new { error = "Unsupported file format", code = "UNSUPPORTED_FORMAT" });

        var result = await _mediator.Send(new UploadContractCommand(file, format), ct);
        if (!result.IsSuccess) return StatusCode(StatusCodes.Status400BadRequest, new { error = result.Error, code = result.ErrorCode });
        return CreatedAtAction(nameof(Get), new { contractId = result.Value!.ContractId }, result.Value);
    }

    [HttpGet("{contractId:guid}")]
    public async Task<IActionResult> Get(Guid contractId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetContractQuery(contractId), ct);
        if (!result.IsSuccess) return NotFound(new { error = result.Error, code = result.ErrorCode });
        return Ok(result.Value);
    }
}
