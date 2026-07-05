using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ClauseLens.Infrastructure.Messaging;

/// <summary>
/// MassTransit configuration. Honors an explicit Transport enum from
/// MassTransitOptions so an empty connection string can't silently fall
/// through to the in-memory transport in Production.
/// </summary>
public static class MassTransitRegistration
{
    public static IServiceCollection AddClauseLensMessaging(this IServiceCollection services, IConfiguration cfg, IHostEnvironment env)
    {
        services.Configure<MassTransitOptions>(o =>
        {
            o.Transport = Enum.TryParse<MessageTransport>(cfg["MassTransitOptions:Transport"], ignoreCase: true, out var t)
                ? t : MessageTransport.InMemory;
            o.ServiceBusConnectionString = cfg["MassTransitOptions:ServiceBusConnectionString"]
                                          ?? cfg.GetConnectionString("ServiceBus");
        });

        services.AddMassTransit(mt =>
        {
            mt.SetKebabCaseEndpointNameFormatter();
            mt.AddEntityFrameworkOutbox<Infrastructure.Persistence.ClauseLensDbContext>(o =>
            {
                o.UseSqlServer();
                o.QueryDelay = TimeSpan.FromSeconds(1);
            });

            // Resolve options at configuration time and validate.
            var opts = new MassTransitOptions
            {
                Transport = Enum.TryParse<MessageTransport>(cfg["MassTransitOptions:Transport"], ignoreCase: true, out var t) ? t : MessageTransport.InMemory,
                ServiceBusConnectionString = cfg["MassTransitOptions:ServiceBusConnectionString"] ?? cfg.GetConnectionString("ServiceBus"),
            };
            MassTransitOptionsValidator.Validate(opts, env);

            if (opts.Transport == MessageTransport.AzureServiceBus)
            {
                mt.UsingAzureServiceBus((ctx, bus) =>
                {
                    bus.Host(opts.ServiceBusConnectionString);
                    bus.ConfigureEndpoints(ctx);
                });
            }
            else
            {
                mt.UsingInMemory((ctx, bus) => bus.ConfigureEndpoints(ctx));
            }
        });
        return services;
    }
}
