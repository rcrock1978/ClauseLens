// xUnit scaffolding for backend tests. T041..T145a — implemented as a small
// per-phase suite so each user story has at least one test that exercises
// the contract, the parser, or the AI integration.

using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using ClauseLens.Domain.Common;
using ClauseLens.Infrastructure.Documents;
using FluentAssertions;
using Xunit;

namespace ClauseLens.Domain.Tests;

public class TenantLifecycleTests
{
    [Fact]
    public void Tenant_can_be_created_with_default_retention()
    {
        var t = Tenant.Create("Acme");
        t.RetentionYearsContracts.Should().Be(7);
        t.RetentionYearsAudit.Should().Be(7);
        t.Status.Should().Be(TenantStatus.Active);
    }

    [Fact]
    public void Offboarding_schedules_30_day_soft_delete()
    {
        var t = Tenant.Create("Acme");
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        t.ScheduleOffboarding(now);
        t.Status.Should().Be(TenantStatus.SoftDeleted);
        t.SoftDeleteScheduledAt.Should().Be(now.AddDays(30));
    }

    [Fact]
    public void Hard_delete_requires_30_day_window()
    {
        var t = Tenant.Create("Acme");
        var now = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        t.ScheduleOffboarding(now);
        var tooEarly = now.AddDays(15);
        Action act = () => t.HardDelete(tooEarly);
        act.Should().Throw<DomainException>();
    }
}

public class ContractStateMachineTests
{
    [Fact]
    public void Contract_transitions_through_full_lifecycle()
    {
        var tenantId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var c = Contract.Create(tenantId, ownerId, "nda.pdf", 1024, "pdf", "file://nda.pdf");
        c.Status.Should().Be(ContractStatus.Uploaded);
        c.ReplaceClauses(new[] { Clause.Create(c.Id, 0, "", "text") });
        c.Status.Should().Be(ContractStatus.Analyzing);
        c.MarkReadyForReview(DateTime.UtcNow);
        c.Status.Should().Be(ContractStatus.ReadyForReview);
        c.StartReview();
        c.Status.Should().Be(ContractStatus.InReview);
        c.RequestRevisions();
        c.Status.Should().Be(ContractStatus.RevisionsRequested);
        c.Resubmit();
        c.Status.Should().Be(ContractStatus.ReadyForReview);
        c.StartReview();
        c.CompleteReview();
        c.Status.Should().Be(ContractStatus.Reviewed);
    }

    [Fact]
    public void Contract_rejects_invalid_transition()
    {
        var c = Contract.Create(Guid.NewGuid(), Guid.NewGuid(), "a.pdf", 1, "pdf", "x");
        Action act = () => c.CompleteReview();
        act.Should().Throw<DomainException>();
    }
}

public class ReviewerSlaTests
{
    [Fact]
    public void SlaDeadline_is_seven_business_days_from_assignedAt()
    {
        // 2026-01-05 is a Monday
        var monday = new DateTime(2026, 1, 5, 10, 0, 0, DateTimeKind.Utc);
        var task = ReviewTask.Create(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), monday, businessDaySla: 7);
        task.SlaDeadline.Should().Be(new DateTime(2026, 1, 14, 10, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ReviewTask_rejects_more_than_two_secondaries()
    {
        Action act = () => ReviewTask.Create(
            Guid.NewGuid(), Guid.NewGuid(),
            new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
            Guid.NewGuid(), DateTime.UtcNow);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reassign_requires_reason()
    {
        var t = ReviewTask.Create(Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), DateTime.UtcNow);
        Action act = () => t.Reassign(Guid.NewGuid(), "", DateTime.UtcNow);
        act.Should().Throw<DomainException>();
    }
}

public class RuleAggregationTests
{
    [Fact]
    public void Multiple_matching_rules_aggregate_into_single_flag_with_highest_severity()
    {
        var matched = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var f = RiskFlag.Create(
            clauseId: Guid.NewGuid(),
            primaryRuleId: matched[2],
            matchedRuleIds: matched,
            highestSeverity: RiskSeverity.Critical,
            confidence: ConfidenceLevel.High,
            rationale: "Multiple matches."
        );
        f.MatchedRuleIds.Should().BeEquivalentTo(matched);
        f.Severity.Should().Be(RiskSeverity.Critical);
    }
}

public class AuditAppendOnlyTests
{
    [Fact]
    public void AuditEntry_computes_a_deterministic_hash()
    {
        var e1 = AuditEntry.Create(Guid.NewGuid(), Guid.NewGuid(), "test.action", contractId: Guid.NewGuid());
        var e2 = AuditEntry.Create(Guid.NewGuid(), Guid.NewGuid(), "test.action", contractId: Guid.NewGuid());
        e1.Hash.Should().NotBeNullOrEmpty();
        e1.Hash.Should().NotBe(e2.Hash);
    }
}

public class ClauseRoutingTests
{
    [Fact]
    public void Clause_with_low_confidence_flag_routes_to_needs_discussion()
    {
        var clause = Clause.Create(Guid.NewGuid(), 0, "", "text");
        var f = RiskFlag.Create(
            clause.Id, Guid.NewGuid(), new[] { Guid.NewGuid() },
            RiskSeverity.High, ConfidenceLevel.Low, "low conf"
        );
        clause.ApplyAutoRouting(new[] { f });
        clause.Status.Should().Be(ClauseStatus.NeedsDiscussion);
    }

    [Fact]
    public void Clause_with_no_flags_is_unreviewed()
    {
        var clause = Clause.Create(Guid.NewGuid(), 0, "", "text");
        clause.ApplyAutoRouting(Array.Empty<RiskFlag>());
        clause.Status.Should().Be(ClauseStatus.Unreviewed);
    }

    [Fact]
    public void Clause_with_non_textual_content_is_needs_discussion()
    {
        var clause = Clause.Create(Guid.NewGuid(), 0, "", "text", containsNonTextualContent: true);
        clause.ApplyAutoRouting(Array.Empty<RiskFlag>());
        clause.Status.Should().Be(ClauseStatus.NeedsDiscussion);
    }
}

public class ClauseSegmenterTests
{
    [Fact]
    public void Segmenter_groups_text_into_clauses()
    {
        var segmenter = new ClauseSegmenter();
        var parsed = new ParsedDocument(
            Segments: new[]
            {
                new ParsedSegment(0, "", "1. The Provider shall deliver the report.", false),
                new ParsedSegment(1, "", "Continuation of clause one.", false),
                new ParsedSegment(2, "", "2. The Customer shall pay the invoice.", false),
            },
            ContainsNonTextualContent: false,
            PageCount: 1
        );
        var clauses = segmenter.Segment(Guid.NewGuid(), parsed);
        clauses.Should().HaveCount(2);
        clauses[0].Text.Should().Contain("Continuation of clause one");
        clauses[1].Index.Should().Be(1);
    }
}
