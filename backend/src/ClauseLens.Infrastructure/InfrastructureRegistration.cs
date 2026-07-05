using ClauseLens.Application.Abstractions;
using ClauseLens.Application.Behaviors;
using ClauseLens.Domain.Entities;
using ClauseLens.Infrastructure.AI;
using ClauseLens.Infrastructure.Audit;
using ClauseLens.Infrastructure.Auth;
using ClauseLens.Infrastructure.Compliance;
using ClauseLens.Infrastructure.Documents;
using ClauseLens.Infrastructure.Email;
using ClauseLens.Infrastructure.Messaging;
using ClauseLens.Infrastructure.Persistence;
using ClauseLens.Infrastructure.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClauseLens.Infrastructure;

/// <summary>
/// Single composition root for all Infrastructure concerns (auth, persistence,
/// messaging, AI, storage, parsers, schedulers, observability). Called by
/// ClauseLens.Api/Program.cs.
/// </summary>
public static class InfrastructureRegistration
{
    public static IServiceCollection AddClauseLensInfrastructure(this IServiceCollection services, IConfiguration cfg)
    {
        // Persistence
        services.AddDbContext<ClauseLensDbContext>(o =>
            o.UseSqlServer(cfg.GetConnectionString("Sql") ?? "Server=localhost,1433;Database=ClauseLens;User Id=sa;Password=DevPassword!23;TrustServerCertificate=true"));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<ITenantRepository, EfTenantRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IPlaybookRepository, EfPlaybookRepository>();
        services.AddScoped<IContractRepository, EfContractRepository>();
        services.AddScoped<IReviewTaskRepository, EfReviewTaskRepository>();
        services.AddScoped<IAuditRepository, EfAuditRepository>();
        services.AddScoped<IRiskFlagRepository, EfRiskFlagRepository>();
        services.AddScoped<IRedlineRepository, EfRedlineRepository>();
        services.AddScoped<IObligationRepository, EfObligationRepository>();
        services.AddScoped<IErasureRequestRepository, EfErasureRequestRepository>();

        // Auth + email
        services.AddSingleton<JwtSettings>(sp =>
        {
            var settings = new JwtSettings
            {
                Secret = cfg["Jwt:Secret"] ?? string.Empty,
                Issuer = cfg["Jwt:Issuer"] ?? "ClauseLens",
                Audience = cfg["Jwt:Audience"] ?? "ClauseLens.Api",
            };
            settings.Validate(sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>());
            return settings;
        });
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();

        // Storage + documents
        services.AddSingleton<IBlobStorageFactory, BlobStorageFactory>();
        services.AddScoped<IBlobStorage>(sp => sp.GetRequiredService<IBlobStorageFactory>().Create());
        services.AddSingleton<IDocumentParser, PdfDocumentParser>();
        services.AddSingleton<IDocumentParser, DocxDocumentParser>();
        services.AddSingleton<ClauseSegmenter>();
        services.AddSingleton<TableTextExtractor>();
        services.AddSingleton<PasswordProtectedDocumentDetector>();

        // AI orchestrator (Anti-Corruption Layer)
        services.Configure<AiServiceOptions>(o => o.BaseUrl = cfg["AiService:BaseUrl"] ?? "http://ai-service:8000");
        services.AddHttpClient<IAiOrchestrator, HttpAiOrchestrator>(c =>
            c.BaseAddress = new Uri(cfg["AiService:BaseUrl"] ?? "http://ai-service:8000"))
            // T185: 3 attempts, exponential backoff + jitter for transient 5xx / 408 / 429.
            .AddStandardResilienceHandler(o =>
            {
                o.Retry.MaxRetryAttempts = 3;
                o.Retry.BackoffType = Polly.BackoffType.Exponential;
                o.Retry.UseJitter = true;
                o.Retry.Delay = TimeSpan.FromMilliseconds(200);
                o.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            });
        services.AddSingleton<IAiProviderTranslator, HttpAiProviderTranslator>();

        // Playbook template seeder (FR-010)
        services.AddSingleton<IPlaybookTemplateSeeder, BuiltInTemplateSeeder>();

        // Audit + scheduling
        services.AddScoped<IAuditEventDispatcher, AuditEventDispatcher>();
        services.AddSingleton<ISystemTenantContext, SystemTenantContext>();
        services.AddHostedService<SlaScheduler>();
        services.AddHostedService<OffboardingScheduler>();

        // Messaging (MassTransit + outbox)
        services.AddClauseLensMessaging(cfg, sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>());

        return services;
    }
}

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly ClauseLensDbContext _db;
    public EfUnitOfWork(ClauseLensDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
