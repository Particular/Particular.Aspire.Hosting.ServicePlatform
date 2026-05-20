namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting.ApplicationModel;

class RabbitMqTransportAnnotation(RabbitMqRouting routing, IResourceWithConnectionString connectionSource) : PlatformTransportAnnotation
{
    /// <inheritdoc />
    public override string TransportType { get; } = $"RabbitMQ.{routing}";

    /// <inheritdoc />
    public override IResourceWithConnectionString ConnectionSource { get; } = connectionSource;
}