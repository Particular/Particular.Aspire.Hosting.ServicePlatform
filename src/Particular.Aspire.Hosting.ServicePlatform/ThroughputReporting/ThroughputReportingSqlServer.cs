namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using System;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Platform;
using Transport;

/// <summary>
/// Configures throughput reporting settings for ServiceControl when using SQL Server transport.
/// https://docs.particular.net/servicecontrol/servicecontrol-instances/configuration#usage-reporting-when-using-the-sqlserver-transport
/// </summary>
public sealed class ThroughputReportingSqlServer(
    IExpressionValue? connectionString = null,
    string? additionalCatalogs = null) : IThroughputReportingProvider
{
    /// <inheritdoc />
    public void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        ArgumentNullException.ThrowIfNull(errorInstance);

        if (errorInstance.Resource.Parent.GetTransport() is not SqlServerTransportAnnotation transport)
        {
            throw new InvalidOperationException(
                $"{nameof(ThroughputReportingSqlServer)} requires the parent platform to be configured with WithTransportSqlServer first.");
        }

        if (connectionString is not null)
        {
            errorInstance.WithEnvironment(
                PlatformEnvironment.ServiceControl.LicensingComponent.SqlServer.ConnectionString,
                connectionString ?? transport.ConnectionSource.ConnectionStringExpression);
        }

        if (additionalCatalogs is not null)
        {
            errorInstance.WithEnvironment(
                PlatformEnvironment.ServiceControl.LicensingComponent.SqlServer.AdditionalCatalogs,
                additionalCatalogs);
        }
    }
}
