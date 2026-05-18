namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using global::Aspire.Hosting.ApplicationModel;

sealed class ThroughputQueueAnnotation(string queueName) : IResourceAnnotation
{
    public string QueueName { get; } = queueName;
}
