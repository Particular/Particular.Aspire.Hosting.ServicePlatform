namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// Adds a transport using the connection string format defined in https://docs.particular.net/servicecontrol/transports#amazon-sqs
/// </summary>
/// <param name="connectionSource">A resource that provides the connection information for Amazon SQS</param>
sealed class AmazonSqsTransportAnnotation(IResourceWithConnectionString connectionSource) : PlatformTransportAnnotation
{
    public override string TransportType { get; } = "AmazonSQS";

    public override IResourceWithConnectionString ConnectionSource => connectionSource;
}
