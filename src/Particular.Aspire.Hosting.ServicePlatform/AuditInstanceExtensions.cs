namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public static class AuditInstanceExtensions
{
    extension(IResourceBuilder<ServiceControlAuditInstanceResource> auditInstance)
    {
        public IResourceBuilder<ServiceControlAuditInstanceResource> WithPersistence(IResourceBuilder<IResource> persistence)
            => auditInstance.WithPersistenceAnnotation(persistence);

        public IResourceBuilder<ServiceControlAuditInstanceResource> WithAuditQueueName(string queueName) =>
            auditInstance.WithEnvironment(ServiceControlAuditInstanceResource.AuditQueueEnvVar, queueName);
    }
}
