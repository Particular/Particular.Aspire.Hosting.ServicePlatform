namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public interface IThroughputReportingProvider
{
    void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance);
}
