using ClauseLens.Application.Abstractions;
using ClauseLens.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClauseLens.Infrastructure.Persistence;

public sealed class EfTenantRepository : ITenantRepository
{
    private readonly ClauseLensDbContext _db;
    public EfTenantRepository(ClauseLensDbContext db) => _db = db;
    public async Task AddAsync(Tenant tenant, CancellationToken ct = default) => await _db.Tenants.AddAsync(tenant, ct);
    public Task<Tenant?> FindByIdAsync(Guid id, CancellationToken ct = default) => _db.Tenants.FirstOrDefaultAsync(t => t.Id == id, ct);
}

public sealed class EfUserRepository : IUserRepository
{
    private readonly ClauseLensDbContext _db;
    public EfUserRepository(ClauseLensDbContext db) => _db = db;
    public async Task AddAsync(User user, CancellationToken ct = default) => await _db.Users.AddAsync(user, ct);
    public Task<User?> FindByIdAsync(Guid id, CancellationToken ct = default) => _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    public Task<User?> FindByEmailAsync(Guid tenantId, string email, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Email == email.ToLowerInvariant(), ct);
    public Task<User?> FindByEmailGlobalAsync(string email, CancellationToken ct = default)
        => _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);
    public Task<User?> FindByVerificationTokenAsync(string token, CancellationToken ct = default)
        => _db.Users.FirstOrDefaultAsync(u => u.EmailVerificationToken == token, ct);
    public Task<bool> EmailExistsInTenantAsync(Guid tenantId, string email, CancellationToken ct = default)
        => _db.Users.AnyAsync(u => u.TenantId == tenantId && u.Email == email.ToLowerInvariant(), ct);
    public Task<bool> EmailExistsGloballyAsync(string email, CancellationToken ct = default)
        => _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);
}

public sealed class EfPlaybookRepository : IPlaybookRepository
{
    private readonly ClauseLensDbContext _db;
    public EfPlaybookRepository(ClauseLensDbContext db) => _db = db;
    public async Task AddAsync(Playbook p, CancellationToken ct = default) => await _db.Playbooks.AddAsync(p, ct);
    public Task<Playbook?> FindByIdAsync(Guid id, CancellationToken ct = default) => _db.Playbooks.Include(p => p.Rules).FirstOrDefaultAsync(p => p.Id == id, ct);
    public async Task<IReadOnlyList<Playbook>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await _db.Playbooks.Include(p => p.Rules).ToListAsync(ct);
    public async Task<IReadOnlyList<PlaybookRule>> ListPublishedRulesForClauseTypeAsync(Guid tenantId, string clauseType, CancellationToken ct = default)
        => await _db.PlaybookRules
            .Where(r => r.IsPublished && r.ClauseType == clauseType && _db.Playbooks.Any(p => p.Id == r.PlaybookId && p.TenantId == tenantId))
            .ToListAsync(ct);
}

public sealed class EfContractRepository : IContractRepository
{
    private readonly ClauseLensDbContext _db;
    public EfContractRepository(ClauseLensDbContext db) => _db = db;
    public async Task AddAsync(Contract c, CancellationToken ct = default) => await _db.Contracts.AddAsync(c, ct);
    public Task<Contract?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Contracts.Include(c => c.Clauses).FirstOrDefaultAsync(c => c.Id == id, ct);
    public async Task ReplaceClausesAsync(Guid contractId, IReadOnlyCollection<Clause> clauses, CancellationToken ct = default)
    {
        var existing = await _db.Clauses.Where(cl => cl.ContractId == contractId).ToListAsync(ct);
        _db.Clauses.RemoveRange(existing);
        await _db.Clauses.AddRangeAsync(clauses, ct);
    }
    public async Task<IReadOnlyList<Contract>> ListByTenantAsync(Guid tenantId, int skip, int take, CancellationToken ct = default)
        => await _db.Contracts.OrderByDescending(c => c.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
}

public sealed class EfReviewTaskRepository : IReviewTaskRepository
{
    private readonly ClauseLensDbContext _db;
    public EfReviewTaskRepository(ClauseLensDbContext db) => _db = db;
    public async Task AddAsync(ReviewTask t, CancellationToken ct = default) => await _db.ReviewTasks.AddAsync(t, ct);
    public Task<ReviewTask?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => _db.ReviewTasks.Include(t => t.Decisions).FirstOrDefaultAsync(t => t.Id == id, ct);
    public Task<ReviewTask?> FindByContractAsync(Guid contractId, CancellationToken ct = default)
        => _db.ReviewTasks.Include(t => t.Decisions).FirstOrDefaultAsync(t => t.ContractId == contractId, ct);
    public async Task<IReadOnlyList<ReviewTask>> ListStallingTasksAsync(DateTime now, CancellationToken ct = default)
    {
        var day3 = now.AddBusinessDays(-3);
        var day7 = now.AddBusinessDays(-7);
        return await _db.ReviewTasks
            .Where(t => t.Status == ReviewTaskStatus.InProgress && t.SlaDeadline <= day7)
            .ToListAsync(ct);
    }
}

public static class DateTimeExtensions
{
    public static DateTime AddBusinessDays(this DateTime start, int businessDays)
    {
        var d = start;
        var added = 0;
        while (added < businessDays)
        {
            d = d.AddDays(1);
            if (d.DayOfWeek != DayOfWeek.Saturday && d.DayOfWeek != DayOfWeek.Sunday) added++;
        }
        return d;
    }
}

public sealed class EfAuditRepository : IAuditRepository
{
    private readonly ClauseLensDbContext _db;
    public EfAuditRepository(ClauseLensDbContext db) => _db = db;
    public async Task AddAsync(AuditEntry e, CancellationToken ct = default) => await _db.AuditEntries.AddAsync(e, ct);
    public Task<string> GetLatestHashAsync(Guid tenantId, CancellationToken ct = default)
        => _db.AuditEntries.Where(a => a.TenantId == tenantId).OrderByDescending(a => a.CreatedAt).Select(a => a.Hash).FirstOrDefaultAsync(ct)
            .ContinueWith(t => t.Result ?? string.Empty, ct);
    public async Task<IReadOnlyList<AuditEntry>> ListAsync(Guid tenantId, Guid? contractId, string? actionType, int skip, int take, CancellationToken ct = default)
    {
        var q = _db.AuditEntries.Where(a => a.TenantId == tenantId);
        if (contractId.HasValue) q = q.Where(a => a.ContractId == contractId);
        if (!string.IsNullOrWhiteSpace(actionType)) q = q.Where(a => a.ActionType == actionType);
        return await q.OrderByDescending(a => a.CreatedAt).Skip(skip).Take(take).ToListAsync(ct);
    }
}

public sealed class EfRiskFlagRepository : IRiskFlagRepository
{
    private readonly ClauseLensDbContext _db;
    public EfRiskFlagRepository(ClauseLensDbContext db) => _db = db;
    public async Task AddRangeAsync(IEnumerable<RiskFlag> flags, CancellationToken ct = default)
        => await _db.RiskFlags.AddRangeAsync(flags, ct);
    public async Task<IReadOnlyList<RiskFlag>> ListByContractAsync(Guid contractId, CancellationToken ct = default)
        => await _db.RiskFlags.Where(f => _db.Clauses.Any(cl => cl.Id == f.ClauseId && cl.ContractId == contractId)).ToListAsync(ct);
    public async Task<IReadOnlyList<(RiskFlag, PlaybookRule)>> ListByContractWithRulesAsync(Guid contractId, CancellationToken ct = default)
    {
        // Join risk flags to the clauses of this contract and to the primary
        // PlaybookRule. The matched-rule list (all secondary rule IDs) is
        // returned on the flag itself; the UI can issue follow-up lookups
        // if it needs the full set of rule text.
        var query =
            from f in _db.RiskFlags
            join cl in _db.Clauses on f.ClauseId equals cl.Id
            join r in _db.PlaybookRules on f.RuleId equals r.Id
            where cl.ContractId == contractId
            select new { Flag = f, Rule = r };
        return await query.Select(x => ValueTuple.Create(x.Flag, x.Rule)).ToListAsync(ct);
    }
    public Task<RiskFlag?> FindByIdAsync(Guid id, CancellationToken ct = default)
        => _db.RiskFlags.FirstOrDefaultAsync(f => f.Id == id, ct);
}

public sealed class EfRedlineRepository : IRedlineRepository
{
    private readonly ClauseLensDbContext _db;
    public EfRedlineRepository(ClauseLensDbContext db) => _db = db;
    public async Task AddRangeAsync(IEnumerable<Redline> redlines, CancellationToken ct = default)
        => await _db.Redlines.AddRangeAsync(redlines, ct);
    public async Task<IReadOnlyList<Redline>> ListByContractAsync(Guid contractId, CancellationToken ct = default)
        => await _db.Redlines.Where(r => _db.RiskFlags.Any(f => f.Id == r.RiskFlagId && _db.Clauses.Any(cl => cl.Id == f.ClauseId && cl.ContractId == contractId))).ToListAsync(ct);
}

public sealed class EfObligationRepository : IObligationRepository
{
    private readonly ClauseLensDbContext _db;
    public EfObligationRepository(ClauseLensDbContext db) => _db = db;
    public async Task AddRangeAsync(IEnumerable<Obligation> obligations, CancellationToken ct = default)
        => await _db.Obligations.AddRangeAsync(obligations, ct);
    public async Task<IReadOnlyList<Obligation>> ListByContractAsync(Guid contractId, CancellationToken ct = default)
        => await _db.Obligations.Where(o => _db.Clauses.Any(cl => cl.Id == o.ClauseId && cl.ContractId == contractId)).ToListAsync(ct);
}

public sealed class EfErasureRequestRepository : IErasureRequestRepository
{
    private readonly ClauseLensDbContext _db;
    public EfErasureRequestRepository(ClauseLensDbContext db) => _db = db;
    public async Task AddAsync(ErasureRequest r, CancellationToken ct = default) => await _db.ErasureRequests.AddAsync(r, ct);
    public async Task<IReadOnlyList<ErasureRequest>> ListPendingPastSlaAsync(DateTime now, CancellationToken ct = default)
        => await _db.ErasureRequests.IgnoreQueryFilters()
            .Where(r => r.Status == ErasureStatus.Pending && r.SlaDeadline <= now)
            .ToListAsync(ct);
}
