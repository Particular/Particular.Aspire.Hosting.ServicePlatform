namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting.ApplicationModel;

sealed class AzureStorageQueuesTransportAnnotation(IResourceWithConnectionString connectionSource)
    : PlatformTransportAnnotation
{
    public override string TransportType => "AzureStorageQueue";

    public override IResourceWithConnectionString ConnectionSource => connectionSource;
}
