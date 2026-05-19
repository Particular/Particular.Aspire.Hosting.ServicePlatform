namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

// Consumer-supplied transport resources (Azure Service Bus, RabbitMQ, etc.) aren't owned by the platform,
// so the topology subscriber can't find them via IResourceWithParent. An annotation on the platform holds
// the connection-string resource directly from configuration-time to BeforeStartEvent.
// For file-based transports (Learning), ConnectionSource is null and StoragePath holds the host directory.

/// <summary>
/// Base class for platform transport annotations that configure a connection-string-based transport
/// (e.g. Azure Service Bus, RabbitMQ). Derive from this class to implement a custom transport provider.
/// </summary>
public abstract class PlatformTransportAnnotation : IPlatformTransportAnnotation
{
    /// <summary>
    /// The transport type identifier passed to ServiceControl via the <c>TRANSPORTTYPE</c> environment variable.
    /// </summary>
    public abstract string TransportType { get; }

    /// <summary>
    /// The Aspire resource that supplies the transport connection string.
    /// </summary>
    public abstract IResourceWithConnectionString ConnectionSource { get; }

    /// <summary>
    /// Applies the transport configuration to the specified resource. For platform components this sets
    /// <c>TRANSPORTTYPE</c> and <c>CONNECTIONSTRING</c> environment variables and adds a wait dependency;
    /// for other resources it injects a connection string reference.
    /// </summary>
    /// <typeparam name="T">The resource type to configure.</typeparam>
    /// <param name="resource">The resource builder to apply transport configuration to.</param>
    public virtual void ApplyTo<T>(IResourceBuilder<T> resource) where T : IResourceWithEnvironment
    {
        if (resource is IResourceBuilder<IResourceWithWaitSupport> waiter)
        {
            waiter.WaitFor(resource.ApplicationBuilder.CreateResourceBuilder(ConnectionSource));
        }

        if (resource is IResourceBuilder<IPlatformComponent>)
        {
            resource.WithEnvironment(context =>
            {
                context.EnvironmentVariables["TRANSPORTTYPE"] = TransportType;
                context.EnvironmentVariables["CONNECTIONSTRING"] = ConnectionSource;
            });
            return;
        }

        resource.WithReference(resource.ApplicationBuilder.CreateResourceBuilder(ConnectionSource));
    }
}