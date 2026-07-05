using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using ClauseLens.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ClauseLens.Application.Contracts.Commands;

public sealed record UploadContractCommand(IFormFile File, string Format) : ICommand<Result<UploadContractResponse>>;

public sealed record UploadContractResponse(Guid ContractId, string Status, int? PageCount);

public sealed class UploadContractValidator : AbstractValidator<UploadContractCommand>
{
    public UploadContractValidator()
    {
        RuleFor(c => c.File).NotNull();
        RuleFor(c => c.File.Length).GreaterThan(0).LessThanOrEqualTo(25 * 1024 * 1024);
        RuleFor(c => c.File.FileName).NotEmpty();
        RuleFor(c => c.Format).Must(f => f == "pdf" || f == "docx")
            .WithMessage("Format must be 'pdf' or 'docx'.");
    }
}

/// <summary>
/// Per FR-001..FR-002a:
///   - Accepts PDF or DOCX (size, format, password-protected enforced).
///   - Refuses > 50 page documents (SC-001) and password-protected files
///     (FR-001a) with clear errors.
///   - Persists the blob, segments the document into Clauses, and routes any
///     clause containing non-textual content to NeedsDiscussion (FR-002a).
/// </summary>
public sealed class UploadContractHandler : IRequestHandler<UploadContractCommand, Result<UploadContractResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IContractRepository _contracts;
    private readonly IBlobStorage _blob;
    private readonly IDocumentParser _parser;
    private readonly ClauseSegmenter _segmenter;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;
    private readonly ILogger<UploadContractHandler> _logger;

    public UploadContractHandler(
        IUnitOfWork uow, IContractRepository contracts, IBlobStorage blob,
        IDocumentParser parser, ClauseSegmenter segmenter,
        ICurrentUser current, IAuditEventDispatcher audit,
        ILogger<UploadContractHandler> logger)
    { _uow = uow; _contracts = contracts; _blob = blob; _parser = parser; _segmenter = segmenter; _current = current; _audit = audit; _logger = logger; }

    public async Task<Result<UploadContractResponse>> Handle(UploadContractCommand cmd, CancellationToken ct)
    {
        if (_current.TenantId is null || _current.UserId is null)
            return Result<UploadContractResponse>.Failure("Authentication required.", "UNAUTHENTICATED");
        if (!_parser.SupportsFormat(cmd.Format))
            return Result<UploadContractResponse>.Failure("Unsupported file format.", "UNSUPPORTED_FORMAT");

        await using var ms = new MemoryStream();
        await cmd.File.OpenReadStream().CopyToAsync(ms, ct);
        ms.Position = 0;

        ParsedDocument parsed;
        try
        {
            parsed = await _parser.ParseAsync(ms, cmd.File.FileName, ct);
        }
        catch (PasswordProtectedDocumentException)
        {
            return Result<UploadContractResponse>.Failure(
                "Password-protected documents are not supported. Please remove the password and re-upload.",
                "PASSWORD_PROTECTED");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Document parse failed for {File}", cmd.File.FileName);
            return Result<UploadContractResponse>.Failure("Document could not be parsed.", "PARSE_FAILED");
        }

        if (parsed.PageCount > 50)
            return Result<UploadContractResponse>.Failure("Document exceeds 50-page limit.", "TOO_LARGE");

        ms.Position = 0;
        var uri = await _blob.UploadAsync(_current.TenantId.Value.ToString(), cmd.File.FileName, ms, cmd.File.ContentType, ct);

        var contract = Contract.Create(_current.TenantId.Value, _current.UserId.Value, cmd.File.FileName, cmd.File.Length, cmd.Format, uri);
        await _contracts.AddAsync(contract, ct);
        var clauses = _segmenter.Segment(contract.Id, parsed);
        await _contracts.ReplaceClausesAsync(contract.Id, clauses, ct);
        contract.ReplaceClauses(clauses);
        contract.MarkReadyForReview(DateTime.UtcNow);

        contract.RaiseDomainEvent(new ContractUploadedDomainEvent(contract.TenantId, contract.Id, contract.OwnerId));
        contract.RaiseDomainEvent(new ContractAnalyzedDomainEvent(contract.TenantId, contract.Id));

        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);

        return Result<UploadContractResponse>.Success(new UploadContractResponse(contract.Id, contract.Status.ToString(), parsed.PageCount > 0 ? parsed.PageCount : null));
    }
}
