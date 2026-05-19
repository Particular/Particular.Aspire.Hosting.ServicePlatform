namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

// Consumer-supplied transport resources (Azure Service Bus, RabbitMQ, etc.) aren't owned by the platform,
// so the topology subscriber can't find them via IResourceWithParent. An annotation on the platform holds
// the connection-string resource directly from configuration-time to BeforeStartEvent.
// For file-based transports (Learning), ConnectionSource is null and StoragePath holds the host directory.

public abstract class PlatformTransportAnnotation : IPlatformTransportAnnotation
{
    public abstract string TransportType { get; }
    public abstract IResourceWithConnectionString ConnectionSource { get; }

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