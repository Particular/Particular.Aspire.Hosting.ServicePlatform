namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting.ApplicationModel;

sealed class AzureServiceBusTransportAnnotation(IResourceWithConnectionString connectionSource)
    : PlatformTransportAnnotation
{
    public override string TransportType => "NetStandardAzureServiceBus";

    public override IResourceWithConnectionString ConnectionSource => connectionSource;
}
