namespace Aspire.Hosting;

using System;
using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;
using Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

public static class ErrorInstanceExtensions
{
    extension(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        public IResourceBuilder<ServiceControlErrorInstanceResource> WithPersistence(IResourceBuilder<IResource> persistence)
            => errorInstance.WithPersistenceAnnotation(persistence);

        public IResourceBuilder<ServiceControlErrorInstanceResource> WithRemoteInstance(IResourceBuilder<ServiceControlAuditInstanceResource> instance) =>
            errorInstance.WithAnnotation(new RemoteInstanceAnnotation(
                    ReferenceExpression.Create(
                        $"{instance.Resource.GetEndpoint(ServiceControlAuditInstanceResource.AuditEndpointName)}"))
                );

        public IResourceBuilder<ServiceControlErrorInstanceResource> WithThroughputQueue(string queueName) =>
            errorInstance
                .WithAnnotation(new ThroughputQueueAnnotation(queueName))
                .WithEnvironment(ServiceControlErrorInstanceResource.ThroughputQueueEnvVar, queueName);

        public IResourceBuilder<ServiceControlErrorInstanceResource> WithErrorQueueName(string queueName) =>
            errorInstance.WithEnvironment(ServiceControlErrorInstanceResource.ErrorQueueEnvVar, queueName);

        public IResourceBuilder<ServiceControlErrorInstanceResource> WithThroughputReporting(IThroughputReportingProvider provider)
        {
            ArgumentNullException.ThrowIfNull(provider);
            provider.ApplyTo(errorInstance);
            return errorInstance.WithAnnotation(new ThroughputReportingAnnotation(provider));
        }
    }
}
