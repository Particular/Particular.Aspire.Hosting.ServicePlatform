namespace Aspire.Hosting;

using System;
using System.Linq;
using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Persistence;
using Particular.Aspire.Hosting.ServicePlatform.Platform;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

/// <summary>
/// Extension methods for composing the Particular Service Platform topology, including ServiceControl,
/// ServicePulse, and related components.
/// </summary>
public static class ParticularPlatformExtensions
{
    // Platform-scoped Add*/With* methods are the public API for composing the platform topology.
    // Child resources are created here so they stay hidden from the distributed app builder surface,
    // while cross-wiring (env vars, wait deps) remains order-independent.
    extension(IResourceBuilder<ParticularPlatformResource> platform)
    {
        /// <summary>
        /// Adds all default platform components (error, audit, monitoring instances and ServicePulse) with
        /// sensible defaults. If transport or persistence have not been configured, Learning transport and
        /// RavenDB persistence are used. Components that have already been added individually are reused
        /// rather than duplicated.
        /// </summary>
        /// <returns>The platform resource builder for chaining.</returns>
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

            var serviceControl = SingleOrAddDefault(() =>
                platform.AddServiceControlErrorInstance(platform.Resource.Name + "-error", persistence)
            );

            var monitoring = SingleOrAddDefault(() =>
                platform.AddServiceControlMonitoringInstance(platform.Resource.Name + "-monitoring")
            );

            SingleOrAddDefault(() =>
                platform.AddServiceControlAuditInstance(platform.Resource.Name + "-audit", serviceControl, persistence)
            );

            SingleOrAddDefault(() =>
                platform.AddServicePulse(platform.Resource.Name + "-servicepulse", serviceControl, monitoring)
            );

            return platform;

            // Local function: only used while composing the default topology. Kept local (rather than a
            // private extension member) so the analyzer can see it is used - IDE0051 does not yet track
            // usages of private members declared inside an extension block.
            IResourceBuilder<T> SingleOrAddDefault<T>(Func<IResourceBuilder<T>> factory)
                where T : IResourceWithParent<ParticularPlatformResource>
            {
                var children = platform.ApplicationBuilder.Resources.OfType<T>().Where(r => r.Parent == platform.Resource).ToList();
                return children switch
                {
                    [{ } x] => platform.ApplicationBuilder.CreateResourceBuilder(x),
                    [] => factory(),
                    _ => throw new Exception("More than one instance of " + typeof(T).Name + " found, cannot determine which one to use for default wiring")
                };
            }
        }

        /// <summary>
        /// Adds a ServicePulse instance to the platform, configured to connect to the specified ServiceControl error instance.
        /// </summary>
        /// <param name="name">The name of the ServicePulse resource in the Aspire application model.</param>
        /// <param name="serviceControl">The ServiceControl error instance that ServicePulse will connect to.</param>
        /// <param name="monitoring">An optional ServiceControl Monitoring instance for real-time monitoring data.</param>
        /// <returns>A resource builder for the ServicePulse resource.</returns>
        public IResourceBuilder<ServicePulseResource> AddServicePulse(string name,
            IResourceBuilder<ServiceControlErrorInstanceResource> serviceControl,
            IResourceBuilder<ServiceControlMonitoringInstanceResource>? monitoring = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentNullException.ThrowIfNull(serviceControl);

            var servicePulse = platform.ApplicationBuilder
                .AddResource(new ServicePulseResource(name, platform.Resource, serviceControl.Resource))
                .WithImage("particular/servicepulse", "latest")
                .WithHttpEndpoint(targetPort: 9090, name: ServicePulseResource.HttpEndpointName)
                .WithUrlForEndpoint(ServicePulseResource.HttpEndpointName, url => url.DisplayText = "ServicePulse")
                .WithRelationship(serviceControl.Resource, "ServiceControl");

            return servicePulse
                .WithMonitoringInstance(monitoring)
                .WithLicense(platform);
        }

        /// <summary>
        /// Adds a ServiceControl Audit instance to the platform. The audit instance is automatically registered
        /// as a remote instance on the specified error instance.
        /// </summary>
        /// <param name="name">The name of the audit instance resource in the Aspire application model.</param>
        /// <param name="serviceControl">The ServiceControl error instance to register this audit instance with.</param>
        /// <param name="persistence">The persistence resource to use, previously registered via a platform persistence extension.</param>
        /// <returns>A resource builder for the ServiceControl Audit instance resource.</returns>
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
                .WithHttpEndpoint(targetPort: 44444, name: ServiceControlAuditInstanceResource.HttpEndpointName)
                .WithUrlForEndpoint(ServiceControlAuditInstanceResource.HttpEndpointName, url =>
                {
                    url.Url += "/api";
                    url.DisplayText = "ServiceControl Audit";
                })
                .WithRunModeArgs()
                .WithHttpHealthCheck("api/configuration", endpointName: ServiceControlAuditInstanceResource.HttpEndpointName);

            serviceControl.WithRemoteInstance(audit);

            return audit.WithLicense(platform)
                .WithTransportFrom(platform)
                .WithPersistence(persistence)
                .WithAuditQueueName(ServiceControlAuditInstanceResource.DefaultAuditQueueName);
        }

        /// <summary>
        /// Adds a ServiceControl Error instance to the platform.
        /// </summary>
        /// <param name="name">The name of the error instance resource in the Aspire application model.</param>
        /// <param name="persistence">The persistence resource to use, previously registered via a platform persistence extension.</param>
        /// <returns>A resource builder for the ServiceControl Error instance resource.</returns>
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
                .WithHttpEndpoint(targetPort: 33333, name: ServiceControlErrorInstanceResource.HttpEndpointName)
                .WithUrlForEndpoint(ServiceControlErrorInstanceResource.HttpEndpointName, url =>
                {
                    url.Url += "/api";
                    url.DisplayText = "ServiceControl Error";
                })
                .WithRunModeArgs()
                .WithHttpHealthCheck("api/configuration", endpointName: ServiceControlErrorInstanceResource.HttpEndpointName);
            return errorInstance
                .WithLicense(platform)
                .WithTransportFrom(platform)
                .WithPersistence(persistence)
                .WithErrorQueueName(ServiceControlErrorInstanceResource.DefaultErrorQueueName);
        }

        /// <summary>
        /// Adds a ServiceControl Monitoring instance to the platform for collecting real-time endpoint performance data.
        /// </summary>
        /// <param name="name">The name of the monitoring instance resource in the Aspire application model.</param>
        /// <returns>A resource builder for the ServiceControl Monitoring instance resource.</returns>
        public IResourceBuilder<ServiceControlMonitoringInstanceResource> AddServiceControlMonitoringInstance(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            var monitoringInstance = platform.ApplicationBuilder
                .AddResource(new ServiceControlMonitoringInstanceResource(name, platform.Resource))
                .WithImage("particular/servicecontrol-monitoring", "latest")
                .WithHttpEndpoint(targetPort: 33633, name: ServiceControlMonitoringInstanceResource.HttpEndpointName)
                .WithUrlForEndpoint(ServiceControlMonitoringInstanceResource.HttpEndpointName, url => url.DisplayText = "ServiceControl Monitoring")
                .WithRunModeArgs()
                .WithHttpHealthCheck("connection", endpointName: ServiceControlMonitoringInstanceResource.HttpEndpointName);

            return monitoringInstance
                .WithLicense(platform)
                .WithTransportFrom(platform);
        }

    }
}
