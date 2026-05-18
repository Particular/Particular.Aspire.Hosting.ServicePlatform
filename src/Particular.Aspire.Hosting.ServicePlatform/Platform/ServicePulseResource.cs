namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents a ServicePulse instance running as a container resource within the Particular Service Platform.
/// ServicePulse provides a web-based UI for monitoring and managing NServiceBus endpoints via ServiceControl.
/// </summary>
public sealed class ServicePulseResource : ContainerResource, IPlatformComponent,
    IResourceWithParent<ParticularPlatformResource>
{
    internal ServicePulseResource([ResourceName] string name,
        ParticularPlatformResource parent,
        ServiceControlErrorInstanceResource errorInstance) : base(name)
    {
        Parent = parent;
        ServiceControlEndpoint =
            new EndpointReference(errorInstance, ServiceControlErrorInstanceResource.ErrorEndpointName);

        Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            context.EnvironmentVariables["SERVICECONTROL_URL"] = ServiceControlEndpoint;
            if (MonitoringEndpoint != null)
            {
                context.EnvironmentVariables["MONITORING_URL"] = MonitoringEndpoint;
            }
            else
            {
                context.EnvironmentVariables["MONITORING_URL"] = "!";
            }
        }));
    }

    internal const string PrimaryEndpointName = "servicepulse";
    /// <summary>
    /// The parent platform resource that this ServicePulse instance belongs to.
    /// </summary>
    public ParticularPlatformResource Parent { get; }

    /// <summary>
    /// The HTTP endpoint reference for the ServiceControl error instance API that ServicePulse connects to.
    /// </summary>
    public EndpointReference ServiceControlEndpoint { get; }

    /// <summary>
    /// The HTTP endpoint reference for the ServiceControl Monitoring instance API, or <c>null</c> if monitoring is not configured.
    /// </summary>
    public EndpointReference? MonitoringEndpoint { get; internal set; }
}
