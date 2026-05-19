namespace Aspire.Hosting;

using System;
using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;
using Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

/// <summary>
/// Extension methods for configuring a ServiceControl Error instance resource.
/// </summary>
public static class ErrorInstanceExtensions
{
    extension(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        /// <summary>
        /// Configures the persistence backend for this error instance.
        /// </summary>
        /// <param name="persistence">The persistence resource to use, previously registered via a platform persistence extension.</param>
        /// <returns>The error instance resource builder for chaining.</returns>
        public IResourceBuilder<ServiceControlErrorInstanceResource> WithPersistence(IResourceBuilder<IResource> persistence)
            => errorInstance.WithPersistenceAnnotation(persistence);

        /// <summary>
        /// Registers a ServiceControl Audit instance as a remote instance of this error instance,
        /// allowing ServiceControl to aggregate data from the audit instance.
        /// </summary>
        /// <param name="instance">The audit instance to register as a remote instance.</param>
        /// <returns>The error instance resource builder for chaining.</returns>
        public IResourceBuilder<ServiceControlErrorInstanceResource> WithRemoteInstance(IResourceBuilder<ServiceControlAuditInstanceResource> instance) =>
            errorInstance.WithAnnotation(new RemoteInstanceAnnotation(
                    ReferenceExpression.Create(
                        $"{instance.Resource.GetEndpoint(ServiceControlAuditInstanceResource.AuditEndpointName)}"))
                );

        /// <summary>
        /// Sets the name of the queue used for throughput data reporting.
        /// </summary>
        /// <param name="queueName">The name of the throughput data queue.</param>
        /// <returns>The error instance resource builder for chaining.</returns>
        public IResourceBuilder<ServiceControlErrorInstanceResource> WithThroughputQueue(string queueName) =>
            errorInstance
                .WithAnnotation(new ThroughputQueueAnnotation(queueName))
                .WithEnvironment(ServiceControlErrorInstanceResource.ThroughputQueueEnvVar, queueName);

        /// <summary>
        /// Sets the name of the error queue that this instance will consume messages from.
        /// </summary>
        /// <param name="queueName">The name of the error queue.</param>
        /// <returns>The error instance resource builder for chaining.</returns>
        public IResourceBuilder<ServiceControlErrorInstanceResource> WithErrorQueueName(string queueName) =>
            errorInstance.WithEnvironment(ServiceControlErrorInstanceResource.ErrorQueueEnvVar, queueName);

        /// <summary>
        /// Configures throughput reporting for this error instance using the specified provider.
        /// </summary>
        /// <param name="provider">The throughput reporting provider to use (e.g., <see cref="ThroughputReportingAzureServiceBus"/>).</param>
        /// <returns>The error instance resource builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> is null.</exception>
        public IResourceBuilder<ServiceControlErrorInstanceResource> WithThroughputReporting(IThroughputReportingProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);
            provider.ApplyTo(errorInstance);
            return errorInstance.WithAnnotation(new ThroughputReportingAnnotation(provider));
        }
    }
}
