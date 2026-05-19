namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using System;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

/// <summary>
/// Configures throughput reporting settings for ServiceControl when using the PostgreSQL transport.
/// https://docs.particular.net/servicecontrol/servicecontrol-instances/configuration#usage-reporting-when-using-the-postgresql-transport
/// </summary>
public sealed class ThroughputReportingPostgreSql : IThroughputReportingProvider
{
    internal const string ConnectionStringEnvVar = "LICENSINGCOMPONENT_POSTGRESQL_CONNECTIONSTRING";

    readonly IExpressionValue? connectionString;

    /// <summary>
    /// Configures throughput reporting settings for ServiceControl when using the PostgreSQL transport.
    /// https://docs.particular.net/servicecontrol/servicecontrol-instances/configuration#usage-reporting-when-using-the-postgresql-transport
    /// </summary>
    /// <param name="connectionString">
    /// A connection string providing at least read access to all queue tables.
    /// If not provided, the transport connection string is used.
    /// </param>
    public ThroughputReportingPostgreSql(IExpressionValue? connectionString = null)
    {
        this.connectionString = connectionString;
    }

    /// <inheritdoc />
    public void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        ArgumentNullException.ThrowIfNull(errorInstance);

        if (errorInstance.Resource.Parent.GetTransport() is not PostgreSqlTransportAnnotation)
        {
            throw new InvalidOperationException(
                $"{nameof(ThroughputReportingPostgreSql)} requires the parent platform to be configured with WithTransportPostgreSql first.");
        }

        if (connectionString is not null)
        {
            errorInstance.WithEnvironment(ConnectionStringEnvVar, connectionString);
        }
    }
}
