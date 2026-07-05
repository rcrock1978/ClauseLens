using System.Diagnostics;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ClauseLens.Infrastructure.Telemetry;

/// <summary>
/// Centralized OpenTelemetry ActivitySource. Distributed tracing follows the
/// Constitution Principle IV mandate: gateway → services → data → AI service.
/// </summary>
public static class ClauseLensTelemetry
{
    public const string ServiceName = "ClauseLens.Api";
    public static readonly ActivitySource Source = new(ServiceName, "1.0.0");

    public static TracerProviderBuilder AddDefaultTracing(this TracerProviderBuilder b)
        => b.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(Source.Name);
}
