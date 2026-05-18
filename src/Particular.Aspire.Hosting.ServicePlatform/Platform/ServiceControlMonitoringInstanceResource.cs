namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents a ServiceControl Monitoring instance running as a container resource within the Particular Service Platform.
/// The monitoring instance collects real-time endpoint performance and health metrics.
/// </summary>
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

    /// <summary>
    /// The parent platform resource that this monitoring instance belongs to.
    /// </summary>
    public ParticularPlatformResource Parent { get; }

    /// <summary>
    /// The HTTP endpoint reference for the monitoring instance API.
    /// </summary>
    public EndpointReference MonitoringEndpoint => new(this, MonitoringEndpointName);
}
