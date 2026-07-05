using ClauseLens.Application.Abstractions;
using ClauseLens.Application.Behaviors;
using ClauseLens.Infrastructure;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Formatting.Compact;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ── Logging (Constitution Principle IV) ───────────────────────────────
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithCorrelationId()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .WriteTo.File(new RenderedCompactJsonFormatter(),
        path: "logs/" + DateTime.UtcNow.ToString("yyyy-MM-dd") + ".json",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

// ── DI ───────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, ClauseLens.Infrastructure.Auth.HttpContextCurrentUser>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ICommand<>).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(AuditBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(ICommand<>).Assembly);

builder.Services.AddClauseLensInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Rate limiting (T167 + T186) ─────────────────────────────────────
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Default per-tenant cap: 600 req/min, used for read paths.
    o.AddPolicy("per-tenant", ctx =>
    {
        var tenant = ctx.User?.FindFirst("tenant_id")?.Value
                     ?? ctx.Connection.RemoteIpAddress?.ToString()
                     ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(tenant, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 600,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });
    });

    // Tighter cap for write paths (uploads, AI analysis). 60 req/min/tenant.
    o.AddPolicy("per-tenant-upload", ctx =>
    {
        var tenant = ctx.User?.FindFirst("tenant_id")?.Value
                     ?? ctx.Connection.RemoteIpAddress?.ToString()
                     ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(tenant, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });
    });

    // Generous cap for read paths: 1200 req/min/tenant.
    o.AddPolicy("per-tenant-read", ctx =>
    {
        var tenant = ctx.User?.FindFirst("tenant_id")?.Value
                     ?? ctx.Connection.RemoteIpAddress?.ToString()
                     ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(tenant, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 1200,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });
    });

    // Per-IP cap for login attempts: 10 req/min.
    o.AddPolicy("per-user-login", ctx =>
    {
        var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });
    });
});

// ── Auth (JWT bearer) ────────────────────────────────────────────────
var jwt = builder.Configuration.GetSection("Jwt").Get<ClauseLens.Infrastructure.Auth.JwtSettings>()
           ?? new ClauseLens.Infrastructure.Auth.JwtSettings { Secret = string.Empty };
jwt.Validate(builder.Environment);
builder.Services.AddSingleton(jwt);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer, ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("per-tenant");
app.MapGet("/health", () => Results.Ok(new { status = "ok", at = DateTime.UtcNow }));

app.Run();
