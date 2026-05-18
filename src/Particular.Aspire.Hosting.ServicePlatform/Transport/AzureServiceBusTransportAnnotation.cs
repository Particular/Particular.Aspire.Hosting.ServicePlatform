namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

sealed class AzureServiceBusTransportAnnotation(IResourceWithConnectionString connectionSource)
    : PlatformTransportAnnotation
{
    public override string TransportType => "NetStandardAzureServiceBus";

    public override IResourceWithConnectionString ConnectionSource => connectionSource;
}
