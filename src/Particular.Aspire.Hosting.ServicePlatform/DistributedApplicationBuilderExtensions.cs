namespace Aspire.Hosting;

using System;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Particular.Aspire.Hosting.ServicePlatform.Licensing;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Extension methods for adding the Particular Service Platform to an Aspire distributed application.
/// </summary>
public static class DistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        /// Adds the Particular Service Platform resource to the distributed application. This is the entry point
        /// for configuring ServiceControl, ServicePulse, and related platform components within an Aspire AppHost.
        /// </summary>
        /// <param name="name">The name of the platform resource in the Aspire application model.</param>
        /// <returns>A resource builder for the platform resource, which can be used to configure transport, persistence, licensing, and platform components.</returns>
        public IResourceBuilder<ParticularPlatformResource> AddParticularPlatform([ResourceName] string name)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentException.ThrowIfNullOrEmpty(name);

            var license = builder.AddParameter(name + "-license", secret: true);

            //this default is set here instead of the `AddParameter()` call, because the latter makes it immutable.
            license.Resource.Default = new ServicePlatformDefaultLicense();

            // Synthetic resources have no backing process, so Aspire can't infer a state. Without
            // WithInitialState the dashboard shows it as undefined; without ExcludeFromManifest it
            // would leak into azd/Bicep publish output despite not being deployable.
            var platform = builder.AddResource(new ParticularPlatformResource(name))
                .WithInitialState(new CustomResourceSnapshot
                {
                    ResourceType = "ParticularPlatform",
                    State = new ResourceStateSnapshot(KnownResourceStates.Starting, KnownResourceStateStyles.Info),
                    Properties = []
                })
                .ExcludeFromManifest()
                .WithAnnotation(new PlatformLicenseAnnotation(license.Resource));


            // Shared readiness state (singleton) + the eventing subscriber that writes to it.
            // Both Try* so duplicate AddParticularPlatform calls in one AppHost don't re-register.
            builder.Services.TryAddSingleton<PlatformReadinessState>();
            builder.Services.TryAddEventingSubscriber<PlatformTopologyEventingSubscriber>();

            return platform;
        }
    }
}
