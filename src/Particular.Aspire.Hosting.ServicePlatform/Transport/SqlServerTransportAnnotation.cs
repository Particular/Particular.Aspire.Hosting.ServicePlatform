namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting.ApplicationModel;

sealed class SqlServerTransportAnnotation(IResourceWithConnectionString connectionSource)
    : PlatformTransportAnnotation
{
    public override string TransportType => "SQLServer";

    public override IResourceWithConnectionString ConnectionSource => connectionSource;
}
