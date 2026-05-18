namespace Aspire.Hosting;

using System;
using System.Linq;
using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Persistence;
using Particular.Aspire.Hosting.ServicePlatform.Platform;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

public static class ParticularPlatformExtensions
{
    // Platform-scoped Add*/With* methods are the public API for composing the platform topology.
    // Child resources are created here so they stay hidden from the distributed app builder surface,
    // while cross-wiring (env vars, wait deps) remains order-independent.
    extension(IResourceBuilder<ParticularPlatformResource> platform)
    {
        public IResourceBuilder<ParticularPlatformResource> AddDefaultComponents()
        {
            // set up transport
            if (!platform.Resource.TryGetLastAnnotation<IPlatformTransportAnnotation>(out _))
            {
                platform.WithTransportLearning();
            }

            // set up persistence
            var platformPersistenceAnnotations = platform.Resource.Annotations
                .OfType<IPlatformPersistenceAnnotation>()
                .ToArray();

            var persistence = platformPersistenceAnnotations switch
            {
                [var persistenceAnnotation] => platform.ApplicationBuilder.CreateResourceBuilder(persistenceAnnotation.Resource),
                [] => platform.AddPersistenceRavenDb(platform.Resource.Name + "-persistence"),
                _ => throw new ArgumentException("Multiple persistence annotations found for platform resource, default setup cannot continue"),
            };

            var serviceControl = platform.SingleOrAddDefault(() =>
                platform.AddServiceControlErrorInstance(platform.Resource.Name + "-error", persistence)
            );

            var monitoring = platform.SingleOrAddDefault(() =>
                platform.AddServiceControlMonitoringInstance(platform.Resource.Name + "-monitoring")
            );

            platform.SingleOrAddDefault(() =>
                platform.AddServiceControlAuditInstance(platform.Resource.Name + "-audit", serviceControl, persistence)
            );

            platform.SingleOrAddDefault(() =>
                platform.AddServicePulse(platform.Resource.Name + "-servicepulse", serviceControl, monitoring)
            );

            return platform;
        }

        private IResourceBuilder<T> SingleOrAddDefault<T>(Func<IResourceBuilder<T>> factory)
            where T : IResourceWithParent<ParticularPlatformResource>
        {
            return platform.ApplicationBuilder.Resources.OfType<T>().ToList() switch
            {
                [{ } x] => platform.ApplicationBuilder.CreateResourceBuilder(x),
                [] => factory(),
                _ => throw new Exception("More than one instance of " + typeof(T).Name + " found, cannot determine which one to use for default wiring")
            };
        }

        public IResourceBuilder<ServicePulseResource> AddServicePulse(string name,
            IResourceBuilder<ServiceControlErrorInstanceResource> serviceControl,
            IResourceBuilder<ServiceControlMonitoringInstanceResource>? monitoring = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(serviceControl);

            var servicePulse = platform.ApplicationBuilder
                .AddResource(new ServicePulseResource(name, platform.Resource, serviceControl.Resource))
                .WithImage("particular/servicepulse", "latest")
                .WithHttpEndpoint(port: 9090, targetPort: 9090, name: ServicePulseResource.PrimaryEndpointName)
                .WithUrlForEndpoint(ServicePulseResource.PrimaryEndpointName, url => url.DisplayText = "ServicePulse");

            return servicePulse
                .WithMonitoringInstance(monitoring)
                .WithLicense(platform);
        }

        public IResourceBuilder<ServiceControlAuditInstanceResource> AddServiceControlAuditInstance(string name,
            IResourceBuilder<ServiceControlErrorInstanceResource> serviceControl,
            IResourceBuilder<IResource> persistence)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(serviceControl);
            ArgumentNullException.ThrowIfNull(persistence);
            if (!platform.Resource.TryGetPersistenceConfig(persistence.Resource, out _))
            {
                throw new ArgumentException($"Persistence resource must be a persistence (e.g platform.{nameof(ParticularServicePlatformPersistenceExtensions.WithPersistenceRavenDb)})", nameof(persistence));
            }

            var audit = platform.ApplicationBuilder
                .AddResource(new ServiceControlAuditInstanceResource(name, platform.Resource))
                .WithImage("particular/servicecontrol-audit", "latest")
                .WithHttpEndpoint(port: 44444, targetPort: 44444, name: ServiceControlAuditInstanceResource.AuditEndpointName)
                .WithUrlForEndpoint(ServiceControlAuditInstanceResource.AuditEndpointName, url => url.DisplayText = "ServiceControl Audit")
                .WithArgs("--setup-and-run")
                .WithHttpHealthCheck("api/configuration", endpointName: ServiceControlAuditInstanceResource.AuditEndpointName);

            serviceControl.WithRemoteInstance(audit);

            return audit.WithLicense(platform)
                .WithTransportFrom(platform)
                .WithPersistence(persistence)
                .WithAuditQueueName(ServiceControlAuditInstanceResource.DefaultAuditQueueName);
        }

        public IResourceBuilder<ServiceControlErrorInstanceResource> AddServiceControlErrorInstance(string name, IResourceBuilder<IResource> persistence)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(persistence);
            if (!platform.Resource.TryGetPersistenceConfig(persistence.Resource, out _))
            {
                throw new ArgumentException($"Persistence resource must be a persistence (e.g platform.{nameof(ParticularServicePlatformPersistenceExtensions.WithPersistenceRavenDb)})", nameof(persistence));
            }

            var errorInstance = platform.ApplicationBuilder
                .AddResource(new ServiceControlErrorInstanceResource(name, platform.Resource))
                .WithImage("particular/servicecontrol", "latest")
                .WithHttpEndpoint(port: 33333, targetPort: 33333, name: ServiceControlErrorInstanceResource.ErrorEndpointName)
                .WithUrlForEndpoint(ServiceControlErrorInstanceResource.ErrorEndpointName, url => url.DisplayText = "ServiceControl Error")
                .WithArgs("--setup-and-run")
                .WithHttpHealthCheck("api/configuration", endpointName: ServiceControlErrorInstanceResource.ErrorEndpointName);
            return errorInstance
                .WithLicense(platform)
                .WithTransportFrom(platform)
                .WithPersistence(persistence)
                .WithErrorQueueName(ServiceControlErrorInstanceResource.DefaultErrorQueueName);
        }

        public IResourceBuilder<ServiceControlMonitoringInstanceResource> AddServiceControlMonitoringInstance(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            var monitoringInstance = platform.ApplicationBuilder
                .AddResource(new ServiceControlMonitoringInstanceResource(name, platform.Resource))
                .WithImage("particular/servicecontrol-monitoring", "latest")
                .WithHttpEndpoint(port: 33633, targetPort: 33633, name: ServiceControlMonitoringInstanceResource.MonitoringEndpointName)
                .WithUrlForEndpoint(ServiceControlMonitoringInstanceResource.MonitoringEndpointName, url => url.DisplayText = "ServiceControl Monitoring")
                .WithArgs("--setup-and-run")
                .WithHttpHealthCheck("connection", endpointName: ServiceControlMonitoringInstanceResource.MonitoringEndpointName);

            return monitoringInstance
                .WithLicense(platform)
                .WithTransportFrom(platform);
        }

    }
}
