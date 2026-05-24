namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Extension methods for configuring a ServiceControl Audit instance resource.
/// </summary>
public static class AuditInstanceExtensions
{
    extension(IResourceBuilder<ServiceControlAuditInstanceResource> auditInstance)
    {
        /// <summary>
        /// Configures the persistence backend for this audit instance.
        /// </summary>
        /// <param name="persistence">The persistence resource to use, previously registered via a platform persistence extension.</param>
        /// <returns>The audit instance resource builder for chaining.</returns>
        public IResourceBuilder<ServiceControlAuditInstanceResource> WithPersistence(IResourceBuilder<IResource> persistence)
            => auditInstance.WithPersistenceAnnotation(persistence);

        /// <summary>
        /// Sets the name of the audit queue that this instance will consume messages from.
        /// </summary>
        /// <param name="queueName">The name of the audit queue.</param>
        /// <returns>The audit instance resource builder for chaining.</returns>
        public IResourceBuilder<ServiceControlAuditInstanceResource> WithAuditQueueName(string queueName) =>
            auditInstance.WithEnvironment(PlatformEnvironment.ServiceControl.AuditQueue, queueName);
    }
}
