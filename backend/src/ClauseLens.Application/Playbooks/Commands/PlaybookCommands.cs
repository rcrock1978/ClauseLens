using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using MediatR;

namespace ClauseLens.Application.Playbooks.Commands;

/// <summary>
/// Per Constitution Principle V: human-in-the-loop gate before a draft
/// PlaybookRule becomes active. Only Admins can publish.
/// </summary>
public sealed record PublishPlaybookRuleCommand(Guid RuleId) : ICommand<Result<PublishPlaybookRuleResponse>>;
public sealed record PublishPlaybookRuleResponse(Guid RuleId, DateTime PublishedAt);

public sealed class PublishPlaybookRuleHandler : IRequestHandler<PublishPlaybookRuleCommand, Result<PublishPlaybookRuleResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IPlaybookRepository _playbooks;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;

    public PublishPlaybookRuleHandler(IUnitOfWork uow, IPlaybookRepository playbooks, ICurrentUser current, IAuditEventDispatcher audit)
    { _uow = uow; _playbooks = playbooks; _current = current; _audit = audit; }

    public async Task<Result<PublishPlaybookRuleResponse>> Handle(PublishPlaybookRuleCommand cmd, CancellationToken ct)
    {
        if (!_current.IsInRole(nameof(UserRole.Admin)))
            return Result<PublishPlaybookRuleResponse>.Failure("Only Admins can publish playbook rules.", "FORBIDDEN");
        var rule = (await _playbooks.ListByTenantAsync(_current.TenantId!.Value, ct))
            .SelectMany(p => p.Rules).FirstOrDefault(r => r.Id == cmd.RuleId);
        if (rule is null) return Result<PublishPlaybookRuleResponse>.Failure("Rule not found.", "NOT_FOUND");
        rule.Publish();
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<PublishPlaybookRuleResponse>.Success(new PublishPlaybookRuleResponse(rule.Id, rule.PublishedAt!.Value));
    }
}

public sealed record ImportPlaybookTemplatesCommand(IReadOnlyList<string> TemplateIds) : ICommand<Result<ImportPlaybookTemplatesResponse>>;
public sealed record ImportPlaybookTemplatesResponse(Guid PlaybookId, int RulesImported);

public sealed class ImportPlaybookTemplatesHandler : IRequestHandler<ImportPlaybookTemplatesCommand, Result<ImportPlaybookTemplatesResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly IPlaybookRepository _playbooks;
    private readonly ICurrentUser _current;
    private readonly IAuditEventDispatcher _audit;
    private readonly IPlaybookTemplateSeeder _seeder;

    public ImportPlaybookTemplatesHandler(IUnitOfWork uow, IPlaybookRepository playbooks, ICurrentUser current, IAuditEventDispatcher audit, IPlaybookTemplateSeeder seeder)
    { _uow = uow; _playbooks = playbooks; _current = current; _audit = audit; _seeder = seeder; }

    public async Task<Result<ImportPlaybookTemplatesResponse>> Handle(ImportPlaybookTemplatesCommand cmd, CancellationToken ct)
    {
        if (_current.TenantId is null) return Result<ImportPlaybookTemplatesResponse>.Failure("Auth required.", "UNAUTHENTICATED");
        var playbook = Playbook.Create(_current.TenantId.Value, "Imported Templates");
        var count = 0;
        foreach (var templateId in cmd.TemplateIds)
            foreach (var r in _seeder.LoadTemplate(templateId))
            {
                playbook.AddRule(r.clauseType, r.condition, r.severity, r.standardLanguage, r.guideline);
                count++;
            }
        await _playbooks.AddAsync(playbook, ct);
        await _uow.SaveChangesAsync(ct);
        await _audit.DispatchPendingAsync(_current, ct);
        return Result<ImportPlaybookTemplatesResponse>.Success(new ImportPlaybookTemplatesResponse(playbook.Id, count));
    }
}

public sealed class BuiltInTemplateSeeder : IPlaybookTemplateSeeder
{
    public IEnumerable<(string clauseType, string condition, RiskSeverity severity, string standardLanguage, string guideline)> LoadTemplate(string templateId) => templateId switch
    {
        "nda" => new[] { ("confidentiality", "indefinite term", RiskSeverity.Medium, "Confidentiality obligations survive for 3 years post-termination.", "Limit survival to a finite term.") },
        "msa" => new[] { ("liability", "unlimited liability", RiskSeverity.Critical, "Liability shall be capped at fees paid in 12 months.", "Cap liability to contract value.") },
        "dpa" => new[] { ("data-processing", "no sub-processor list", RiskSeverity.High, "Sub-processors must be disclosed in Annex II.", "Require sub-processor list per GDPR Art. 28(2).") },
        _ => Array.Empty<(string, string, RiskSeverity, string, string)>(),
    };
}
