namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Defines a provider that configures throughput reporting on a ServiceControl Error instance.
/// Implementations supply the transport-specific environment variables needed for throughput data collection.
/// </summary>
public interface IThroughputReportingProvider
{
    /// <summary>
    /// Applies the throughput reporting configuration to the specified error instance.
    /// </summary>
    /// <param name="errorInstance">The ServiceControl Error instance to configure.</param>
    void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance);
}
