namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting.ApplicationModel;

sealed class PostgreSqlTransportAnnotation(IResourceWithConnectionString connectionSource)
    : PlatformTransportAnnotation
{
    public override string TransportType => "PostgreSQL";

    public override IResourceWithConnectionString ConnectionSource => connectionSource;
}
