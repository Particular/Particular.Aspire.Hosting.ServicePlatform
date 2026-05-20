namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

/// <summary>
/// RabbitMQ routing types. More information can be found at https://docs.particular.net/servicecontrol/transports#rabbitmq
/// </summary>
public enum RabbitMqRouting
{
    /// <summary>
    /// Use Quorum Queues (https://www.rabbitmq.com/docs/quorum-queues), with a
    /// Conventional Routing Topology (https://docs.particular.net/transports/rabbitmq/routing-topology#conventional-routing-topology).
    /// </summary>
    QuorumConventionalRouting,

    /// <summary>
    /// Use Classic Queues (https://www.rabbitmq.com/docs/classic-queues), with a
    /// Conventional Routing Topology (https://docs.particular.net/transports/rabbitmq/routing-topology#conventional-routing-topology).
    /// </summary>
    ClassicConventionalRouting,

    /// <summary>
    /// Use Quorum Queues (https://www.rabbitmq.com/docs/quorum-queues), with a
    /// Direct Routing Topology (https://docs.particular.net/transports/rabbitmq/routing-topology#direct-routing-topology).
    /// </summary>
    QuorumDirectRouting,

    /// <summary>
    /// Use Classic Queues (https://www.rabbitmq.com/docs/classic-queues), with a
    /// Direct Routing Topology (https://docs.particular.net/transports/rabbitmq/routing-topology#direct-routing-topology).
    /// </summary>
    ClassicDirectRouting,
}