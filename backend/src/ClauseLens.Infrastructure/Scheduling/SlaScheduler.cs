using ClauseLens.Application.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClauseLens.Infrastructure.Scheduling;

/// <summary>
/// Background service that runs every 30 minutes to:
///   1. Fire day-3 nudges on InProgress review tasks that haven't been nudged.
///   2. Allow Owners to reassign tasks past day 7 (the API endpoint enforces).
/// Per FR-007b.
/// </summary>
public sealed class SlaScheduler : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SlaScheduler> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(30);

    public SlaScheduler(IServiceProvider services, ILogger<SlaScheduler> logger)
    { _services = services; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await TickAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "SLA scheduler tick failed"); }
            try { await Task.Delay(_interval, stoppingToken); } catch (OperationCanceledException) { break; }
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var reviewTasks = scope.ServiceProvider.GetRequiredService<IReviewTaskRepository>();
        var contracts = scope.ServiceProvider.GetRequiredService<IContractRepository>();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailSender>();
        var systemTenant = scope.ServiceProvider.GetRequiredService<ISystemTenantContext>();
        var audit = scope.ServiceProvider.GetRequiredService<IAuditEventDispatcher>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var now = DateTime.UtcNow;

        var stalling = await reviewTasks.ListStallingTasksAsync(now, ct);
        foreach (var task in stalling)
        {
            // Per FR-007b: nudge at day 3, reassign allowed at day 7.
            // Day 3 = assignedAt + 3 business days; day 7 = assignedAt + 7 business days.
            var day3 = task.AssignedAt.AddBusinessDays(3);
            var day7 = task.AssignedAt.AddBusinessDays(7);

            if (task.SlaNudgedAt is null && now >= day3 && now < day7)
            {
                task.RecordSlaNudge(now);

                // Resolve the Contract Owner's email and fire the nudge.
                var contract = await contracts.FindByIdAsync(task.ContractId, ct);
                if (contract is not null)
                {
                    systemTenant.SetCurrentTenant(task.ContractId == Guid.Empty ? Guid.Empty : contract.TenantId);
                    var owner = contract.OwnerId != Guid.Empty ? await users.FindByIdAsync(contract.OwnerId, ct) : null;
                    if (owner is not null)
                        await email.SendReviewerSlaNudgeAsync(owner.Email, task.ContractId, day7, ct);
                    systemTenant.Clear();
                }
            }
        }
        await uow.SaveChangesAsync(ct);
        await audit.DispatchPendingAsync(new SystemCurrentUser { UserId = Guid.Empty }, ct);
    }
}
