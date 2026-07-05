using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Common;
using ClauseLens.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClauseLens.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext. Applies a global query filter for tenant isolation (FR-011,
/// SC-006). The current-tenant scope is supplied by ICurrentUser; the filter
/// resolves to a no-op when no tenant is set (system background jobs).
/// </summary>
public sealed class ClauseLensDbContext : DbContext
{
    private readonly ICurrentUser _current;

    public ClauseLensDbContext(DbContextOptions<ClauseLensDbContext> options, ICurrentUser current)
        : base(options)
    { _current = current; }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Playbook> Playbooks => Set<Playbook>();
    public DbSet<PlaybookRule> PlaybookRules => Set<PlaybookRule>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Clause> Clauses => Set<Clause>();
    public DbSet<RiskFlag> RiskFlags => Set<RiskFlag>();
    public DbSet<Redline> Redlines => Set<Redline>();
    public DbSet<Obligation> Obligations => Set<Obligation>();
    public DbSet<ReviewTask> ReviewTasks => Set<ReviewTask>();
    public DbSet<ClauseDecision> ClauseDecisions => Set<ClauseDecision>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<ErasureRequest> ErasureRequests => Set<ErasureRequest>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Tenant-scoped entities get a global query filter
        var tenantId = _current.TenantId;
        if (tenantId.HasValue)
        {
            b.Entity<User>().HasQueryFilter(u => u.TenantId == tenantId);
            b.Entity<Playbook>().HasQueryFilter(p => p.TenantId == tenantId);
            b.Entity<Contract>().HasQueryFilter(c => c.TenantId == tenantId);
            b.Entity<AuditEntry>().HasQueryFilter(a => a.TenantId == tenantId);
        }

        // Map the owned Clause collection
        b.Entity<Contract>().OwnsMany(c => c.Clauses, cb =>
        {
            cb.WithOwner().HasForeignKey(cl => cl.ContractId);
            cb.HasKey(cl => cl.Id);
            cb.Property(cl => cl.Heading).HasMaxLength(500);
            cb.Property(cl => cl.Text).IsRequired();
            cb.Property(cl => cl.SystemNote).HasMaxLength(1000);
        });

        b.Entity<ReviewTask>().OwnsMany(r => r.Decisions, db =>
        {
            db.WithOwner().HasForeignKey(d => d.ReviewTaskId);
            db.HasKey(d => d.Id);
        });

        b.Entity<Playbook>().HasMany(p => p.Rules).WithOne().HasForeignKey(r => r.PlaybookId);
        b.Entity<RiskFlag>().HasIndex(f => f.ClauseId);
        b.Entity<RiskFlag>().HasIndex(f => f.RuleId);
        b.Entity<Redline>().HasIndex(r => r.RiskFlagId);
        b.Entity<Obligation>().HasIndex(o => o.ClauseId);
        b.Entity<ClauseDecision>().HasIndex(d => new { d.ReviewTaskId, d.ClauseId }).IsUnique();
        b.Entity<AuditEntry>().HasIndex(a => new { a.TenantId, a.CreatedAt });
        b.Entity<ErasureRequest>().HasIndex(e => new { e.TenantId, e.SlaDeadline });
    }
}
