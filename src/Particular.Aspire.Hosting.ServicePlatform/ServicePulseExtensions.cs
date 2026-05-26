namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Extension methods for configuring a ServicePulse resource.
/// </summary>
public static class ServicePulseExtensions
{
    extension(IResourceBuilder<ServicePulseResource> servicePulse)
    {
        /// <summary>
        /// Connects a ServiceControl Monitoring instance to ServicePulse for displaying real-time
        /// endpoint performance data.
        /// </summary>
        /// <param name="monitoring">The monitoring instance to connect, or <c>null</c> to disable monitoring integration.</param>
        /// <returns>The ServicePulse resource builder for chaining.</returns>
        public IResourceBuilder<ServicePulseResource> WithMonitoringInstance(
            IResourceBuilder<ServiceControlMonitoringInstanceResource>? monitoring = null)
        {
            servicePulse.Resource.MonitoringEndpoint = monitoring?.Resource.MonitoringEndpoint;
            if (monitoring is not null)
            {
                servicePulse.WithRelationship(monitoring.Resource, "Monitoring");
            }
            return servicePulse;
        }
    }
}