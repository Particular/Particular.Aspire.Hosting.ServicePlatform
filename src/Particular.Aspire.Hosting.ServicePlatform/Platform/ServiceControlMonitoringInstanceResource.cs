namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using global::Aspire.Hosting.ApplicationModel;

public sealed class ServiceControlMonitoringInstanceResource : ContainerResource, IPlatformComponent, IResourceWithParent<ParticularPlatformResource>
{
    internal const string MonitoringEndpointName = "monitoring";
    internal const string ThroughputQueueEnvVar = "MONITORING_SERVICECONTROLTHROUGHPUTDATAQUEUE";
    internal const string MonitoringQueueEnvVar = "MONITORING_INSTANCENAME";
    internal const string DefaultMonitoringQueueName = "Particular.Monitoring";

    internal ServiceControlMonitoringInstanceResource([ResourceName] string name, ParticularPlatformResource parent)
        : base(name)
    {
        Parent = parent;
    }

    public ParticularPlatformResource Parent { get; }

    public EndpointReference MonitoringEndpoint => new(this, MonitoringEndpointName);
}
