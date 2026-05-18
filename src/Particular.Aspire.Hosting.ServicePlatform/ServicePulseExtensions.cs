namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public static class ServicePulseExtensions
{
    extension(IResourceBuilder<ServicePulseResource> servicePulse)
    {
        public IResourceBuilder<ServicePulseResource> WithMonitoringInstance(
            IResourceBuilder<ServiceControlMonitoringInstanceResource>? monitoring = null)
        {
            servicePulse.Resource.MonitoringEndpoint = monitoring?.Resource.MonitoringEndpoint;
            return servicePulse;
        }
    }
}