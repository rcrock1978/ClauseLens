namespace ClauseLens.Infrastructure.Messaging;

public enum MessageTransport
{
    /// <summary>Use the in-memory transport. For tests and local dev only.</summary>
    InMemory = 0,
    /// <summary>Use Azure Service Bus. Required for any non-Development environment.</summary>
    AzureServiceBus = 1,
}

public sealed class MassTransitOptions
{
    public MessageTransport Transport { get; set; } = MessageTransport.InMemory;
    public string? ServiceBusConnectionString { get; set; }
}

public static class MassTransitOptionsValidator
{
    public static void Validate(MassTransitOptions options, Microsoft.Extensions.Hosting.IHostEnvironment env)
    {
        if (env.IsProduction() && options.Transport != MessageTransport.AzureServiceBus)
            throw new InvalidOperationException(
                "MassTransitOptions.Transport must be AzureServiceBus in Production. " +
                "Configure MassTransitOptions__Transport=AzureServiceBus and " +
                "MassTransitOptions__ServiceBusConnectionString.");
        if (options.Transport == MessageTransport.AzureServiceBus && string.IsNullOrWhiteSpace(options.ServiceBusConnectionString))
            throw new InvalidOperationException(
                "MassTransitOptions.ServiceBusConnectionString is required when Transport=AzureServiceBus.");
    }
}
