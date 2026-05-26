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
        /// Adds the container argument for the resource's configured <see cref="PlatformRunMode"/>
        /// (<c>--setup-and-run</c>, <c>--setup</c>, or none), defaulting to
        /// <see cref="PlatformRunMode.SetupAndRun"/> when unset. The argument is applied via a callback
        /// so the decision is evaluated at build/publish time and is order-independent relative to
        /// WithRunMode().
        /// </summary>
        internal IResourceBuilder<T> WithRunModeArgs() =>
            resource.WithArgs(context =>
            {
                var mode = resource.Resource.TryGetLastAnnotation<RunModeAnnotation>(out var annotation)
                    ? annotation.Mode
                    : PlatformRunMode.SetupAndRun;

                var runModeArg = mode switch
                {
                    PlatformRunMode.SetupAndRun => "--setup-and-run",
                    PlatformRunMode.Setup => "--setup",
                    PlatformRunMode.Run => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(annotation), mode, "Unknown platform run mode")
                };

                if (runModeArg is not null)
                {
                    context.Args.Add(runModeArg);
                }
            });
    }
}