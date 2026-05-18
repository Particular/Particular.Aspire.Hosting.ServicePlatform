namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using global::Aspire.Hosting.ApplicationModel;

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
    public ParticularPlatformResource Parent { get; }
    public EndpointReference ServiceControlEndpoint { get; }
    public EndpointReference? MonitoringEndpoint { get; internal set; }
}
