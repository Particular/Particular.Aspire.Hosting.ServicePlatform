namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using System;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

/// <summary>
/// Configures throughput reporting settings for ServiceControl when using the SQL Server transport.
/// https://docs.particular.net/servicecontrol/servicecontrol-instances/configuration#usage-reporting-when-using-the-sqlserver-transport
/// </summary>
public sealed class ThroughputReportingSqlServer : IThroughputReportingProvider
{
    internal const string ConnectionStringEnvVar = "LICENSINGCOMPONENT_SQLSERVER_CONNECTIONSTRING";
    internal const string AdditionalCatalogsEnvVar = "LICENSINGCOMPONENT_SQLSERVER_ADDITIONALCATALOGS";

    readonly IExpressionValue? connectionString;
    readonly IExpressionValue? additionalCatalogs;

    /// <summary>
    /// Configures throughput reporting settings for ServiceControl when using the SQL Server transport.
    /// https://docs.particular.net/servicecontrol/servicecontrol-instances/configuration#usage-reporting-when-using-the-sqlserver-transport
    /// </summary>
    /// <param name="connectionString">
    /// A connection string providing at least read access to all queue tables.
    /// If not provided, the transport connection string is used.
    /// </param>
    /// <param name="additionalCatalogs">
    /// Comma-separated additional database names on the same server that also contain NServiceBus message queues.
    /// </param>
    public ThroughputReportingSqlServer(
        IExpressionValue? connectionString = null,
        IExpressionValue? additionalCatalogs = null)
    {
        this.connectionString = connectionString;
        this.additionalCatalogs = additionalCatalogs;
    }

    /// <inheritdoc />
    public void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        ArgumentNullException.ThrowIfNull(errorInstance);

        if (errorInstance.Resource.Parent.GetTransport() is not SqlServerTransportAnnotation)
        {
            throw new InvalidOperationException(
                $"{nameof(ThroughputReportingSqlServer)} requires the parent platform to be configured with WithTransportSqlServer first.");
        }

        if (connectionString is not null)
        {
            errorInstance.WithEnvironment(ConnectionStringEnvVar, connectionString);
        }

        if (additionalCatalogs is not null)
        {
            errorInstance.WithEnvironment(AdditionalCatalogsEnvVar, additionalCatalogs);
        }
    }
}
