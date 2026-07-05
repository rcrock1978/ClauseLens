using ClauseLens.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClauseLens.Infrastructure.Compliance;

/// <summary>
/// Daily background job that:
///   1. Hard-deletes tenants whose 30-day soft-delete window has elapsed (FR-013).
///   2. Processes pending ErasureRequest rows whose SLA has elapsed.
/// </summary>
public sealed class OffboardingScheduler : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OffboardingScheduler> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public OffboardingScheduler(IServiceProvider services, ILogger<OffboardingScheduler> logger)
    { _services = services; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Offboarding tick failed"); }
            try { await Task.Delay(_interval, stoppingToken); } catch (OperationCanceledException) { break; }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        await HardDeleteOverdueTenantsAsync(ct);
        await ProcessOverdueErasuresAsync(ct);
    }

    private async Task HardDeleteOverdueTenantsAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Persistence.ClauseLensDbContext>();
        var tenants = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var systemTenant = scope.ServiceProvider.GetRequiredService<ISystemTenantContext>();
        var audit = scope.ServiceProvider.GetRequiredService<IAuditEventDispatcher>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateTime.UtcNow;

        var overdue = await db.Tenants
            .IgnoreQueryFilters()
            .Where(t => t.Status == TenantStatus.SoftDeleted && t.SoftDeleteScheduledAt != null && t.SoftDeleteScheduledAt <= now)
            .ToListAsync(ct);

        foreach (var tenant in overdue)
        {
            systemTenant.SetCurrentTenant(tenant.Id);
            tenant.HardDelete(now);
        }
        await uow.SaveChangesAsync(ct);
        if (overdue.Count > 0)
        {
            _logger.LogInformation("Hard-deleted {Count} tenant(s) past the 30-day window", overdue.Count);
            await audit.DispatchPendingAsync(new SystemCurrentUser(), ct);
        }
        systemTenant.Clear();
    }

    private async Task ProcessOverdueErasuresAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var erasure = scope.ServiceProvider.GetRequiredService<IErasureRequestRepository>();
        var systemTenant = scope.ServiceProvider.GetRequiredService<ISystemTenantContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var audit = scope.ServiceProvider.GetRequiredService<IAuditEventDispatcher>();
        var now = DateTime.UtcNow;

        var overdue = await erasure.ListPendingPastSlaAsync(now, ct);
        foreach (var request in overdue)
        {
            systemTenant.SetCurrentTenant(request.TenantId);
            request.MarkCompleted(now);
        }
        await uow.SaveChangesAsync(ct);
        if (overdue.Count > 0)
        {
            _logger.LogInformation("Completed {Count} erasure request(s) past the 30-day SLA", overdue.Count);
            await audit.DispatchPendingAsync(new SystemCurrentUser(), ct);
        }
        systemTenant.Clear();
    }
}
