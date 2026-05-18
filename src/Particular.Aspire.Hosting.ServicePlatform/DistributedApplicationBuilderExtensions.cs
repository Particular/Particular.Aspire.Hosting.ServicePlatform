namespace Aspire.Hosting;

using System;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Particular.Aspire.Hosting.ServicePlatform.Licensing;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public static class DistributedApplicationBuilderExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
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
