namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using Particular.Aspire.Hosting.ServicePlatform.Persistence;
using global::Aspire.Hosting.ApplicationModel;
using System;

/// <summary>
/// Represents a ServiceControl Audit instance running as a container resource within the Particular Service Platform.
/// The audit instance ingests audit messages and provides audit data to ServiceControl.
/// </summary>
public sealed class ServiceControlAuditInstanceResource : ContainerResource, IPlatformComponent, IResourceWithParent<ParticularPlatformResource>
{
    internal ServiceControlAuditInstanceResource([ResourceName] string name, ParticularPlatformResource parent) : base(name)
    {
        Parent = parent;
        Annotations.Add(new EnvironmentCallbackAnnotation(context =>
            {
                if (!this.TryGetLastAnnotation<IPlatformPersistenceAnnotation>(out var annotation))
                {
                    throw new InvalidOperationException($"No persistence found for {Name}");
                }

                annotation.ApplyConfig(context);
            }));
    }

    internal const string HttpEndpointName = "http";
    internal const string DefaultAuditQueueName = "audit";
    /// <summary>
    /// The parent platform resource that this audit instance belongs to.
    /// </summary>
    public ParticularPlatformResource Parent { get; }
}
