namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Extension methods for configuring a ServiceControl Monitoring instance resource.
/// </summary>
public static class MonitoringInstanceExtensions
{
    extension(IResourceBuilder<ServiceControlMonitoringInstanceResource> monitoring)
    {
        /// <summary>
        /// Configures the throughput data queue for this monitoring instance to match the queue configured
        /// on the specified error instance.
        /// </summary>
        /// <param name="errorInstance">The error instance whose throughput queue configuration should be used.</param>
        /// <returns>The monitoring instance resource builder for chaining.</returns>
        public IResourceBuilder<ServiceControlMonitoringInstanceResource> WithThroughputQueueFrom(
            IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance) =>
            monitoring.WithEnvironment(ctx =>
            {
                if (errorInstance.Resource.TryGetLastAnnotation<ThroughputQueueAnnotation>(out var throughput))
                {
                    ctx.EnvironmentVariables[ServiceControlMonitoringInstanceResource.ThroughputQueueEnvVar] = throughput.QueueName;
                }
            });

        /// <summary>
        /// Sets the name of the throughput data queue for this monitoring instance. Use this when the queue address
        /// needs to differ from the error instance, for example when using MSMQ with monitoring on a different machine.
        /// </summary>
        /// <param name="queueName">The name of the throughput data queue.</param>
        /// <returns>The monitoring instance resource builder for chaining.</returns>
        /// <seealso href="https://docs.particular.net/servicecontrol/monitoring-instances/configuration#usage-reporting-monitoringservicecontrolthroughputdataqueue"/>
        public IResourceBuilder<ServiceControlMonitoringInstanceResource> WithThroughputQueue(
            string queueName) =>
            monitoring.WithEnvironment(ctx =>
            {
                ctx.EnvironmentVariables[ServiceControlMonitoringInstanceResource.ThroughputQueueEnvVar] = queueName;
            });

        /// <summary>
        /// Sets the name of the monitoring queue that this instance will consume messages from.
        /// </summary>
        /// <param name="queueName">The name of the monitoring queue.</param>
        /// <returns>The monitoring instance resource builder for chaining.</returns>
        public IResourceBuilder<ServiceControlMonitoringInstanceResource> WithMonitoringQueueName(string queueName) =>
            monitoring
                .WithEnvironment(ServiceControlMonitoringInstanceResource.MonitoringQueueEnvVar, queueName);
    }
}
