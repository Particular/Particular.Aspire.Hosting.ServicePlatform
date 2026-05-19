namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting.ApplicationModel;

sealed class IbmMqTransportAnnotation(IResourceWithConnectionString connectionSource)
    : PlatformTransportAnnotation
{
    public override string TransportType => "IBMMQ";

    public override IResourceWithConnectionString ConnectionSource => connectionSource;
}
