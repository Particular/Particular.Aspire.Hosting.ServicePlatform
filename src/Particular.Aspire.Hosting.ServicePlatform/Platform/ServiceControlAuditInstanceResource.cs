namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using Particular.Aspire.Hosting.ServicePlatform.Persistence;
using global::Aspire.Hosting.ApplicationModel;
using System;

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

    internal const string AuditEndpointName = "audit";
    internal const string AuditQueueEnvVar = "SERVICEBUS_AUDITQUEUE";
    internal const string DefaultAuditQueueName = "audit";
    public ParticularPlatformResource Parent { get; }
}
