using ClauseLens.Domain.Common;

namespace ClauseLens.Domain.Entities;

/// <summary>
/// A collection of <see cref="PlaybookRule"/>s scoped to a tenant, organized by
/// clause type. Retained while the tenant is active (per FR-013).
/// </summary>
public class Playbook : AggregateRoot
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsTemplate { get; private set; }

    private readonly List<PlaybookRule> _rules = new();
    public IReadOnlyCollection<PlaybookRule> Rules => _rules.AsReadOnly();

    private Playbook() { }

    public static Playbook Create(Guid tenantId, string name, bool isTemplate = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Playbook name is required.");
        return new Playbook
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name.Trim(),
            IsTemplate = isTemplate,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public PlaybookRule AddRule(string clauseType, string condition, RiskSeverity severity,
                                 string standardLanguage, string guideline)
    {
        var rule = PlaybookRule.Create(Id, clauseType, condition, severity, standardLanguage, guideline);
        _rules.Add(rule);
        UpdatedAt = DateTime.UtcNow;
        return rule;
    }
}

/// <summary>
/// A specific standard for a clause type. Severity is one of Low/Medium/High/Critical.
/// </summary>
public class PlaybookRule : Entity
{
    public Guid PlaybookId { get; private set; }
    public string ClauseType { get; private set; } = string.Empty;
    public string Condition { get; private set; } = string.Empty;
    public RiskSeverity Severity { get; private set; }
    public string StandardLanguage { get; private set; } = string.Empty;
    public string Guideline { get; private set; } = string.Empty;
    public bool IsPublished { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    private PlaybookRule() { }

    public static PlaybookRule Create(Guid playbookId, string clauseType, string condition,
                                       RiskSeverity severity, string standardLanguage, string guideline)
    {
        if (string.IsNullOrWhiteSpace(clauseType) || clauseType.Length > 100)
            throw new DomainException("ClauseType must be 1-100 chars.");
        if (string.IsNullOrWhiteSpace(standardLanguage))
            throw new DomainException("StandardLanguage is required.");

        return new PlaybookRule
        {
            Id = Guid.NewGuid(),
            PlaybookId = playbookId,
            ClauseType = clauseType.Trim(),
            Condition = condition?.Trim() ?? string.Empty,
            Severity = severity,
            StandardLanguage = standardLanguage.Trim(),
            Guideline = guideline?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public void Publish()
    {
        if (IsPublished) return;
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum RiskSeverity
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3,
}
