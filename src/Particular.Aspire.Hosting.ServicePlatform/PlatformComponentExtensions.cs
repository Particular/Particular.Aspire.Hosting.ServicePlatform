namespace Aspire.Hosting;

using System;
using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

static class PlatformComponentExtensions
{
    extension<T>(IResourceBuilder<T> resource)
        where T : IResource, IResourceWithWaitSupport, IPlatformComponent,
        IResourceWithParent<ParticularPlatformResource>
    {
        /// <summary>
        /// Internal implementation of WithPersistence with a looser type signature than the public api
        /// See <see cref="ErrorInstanceExtensions.WithPersistence"/> or <see cref="AuditInstanceExtensions.WithPersistence"/>
        /// </summary>
        internal IResourceBuilder<T> WithPersistenceAnnotation(IResourceBuilder<IResource> persistence)
        {
            if (!resource.Resource.Parent.TryGetPersistenceConfig(persistence.Resource, out var persistenceConfig))
            {
                throw new Exception($"Resource '{persistence.Resource.Name}' is not a platform persistence. Persistence resources must be registered via platform.WithPersistenceXXX() extensions first");
            }
            return resource
                //must use wait annotation explicitly inside the platform hierarchy
                .WithAnnotation(new WaitAnnotation(persistence.Resource, WaitType.WaitUntilHealthy) { WaitBehavior = WaitBehavior.WaitOnResourceUnavailable })
                .WithRelationship(persistence.Resource, "Persistence")
                .WithAnnotation(persistenceConfig);
        }
    }

    extension<T>(IResourceBuilder<T> resource)
        where T : IResource, IResourceWithArgs
    {
        /// <summary>
        /// Adds the "--setup-and-run" container argument unless the resource is marked to skip setup
        /// via WithoutSetup(). The argument is applied via a callback so the
        /// decision is evaluated at build/publish time and is order-independent relative to WithoutSetup().
        /// </summary>
        internal IResourceBuilder<T> WithSetupAndRunArgs() =>
            resource.WithArgs(context =>
            {
                if (!resource.Resource.TryGetLastAnnotation<SkipSetupAnnotation>(out _))
                {
                    context.Args.Add("--setup-and-run");
                }
            });
    }
}