namespace Particular.Aspire.ServicePlatform.Platform;

using global::Aspire.Hosting.ApplicationModel;
using Hosting.ServicePlatform.Transport;

sealed class AmazonSqsTransportAnnotation(IResourceWithConnectionString connectionSource) : PlatformTransportAnnotation
{
    public override string TransportType { get; } = "AmazonSQS";
    public override IResourceWithConnectionString ConnectionSource => connectionSource;
}
