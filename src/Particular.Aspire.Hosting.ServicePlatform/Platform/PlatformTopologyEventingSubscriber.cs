namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using global::Aspire.Hosting.Eventing;
using global::Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.Logging;

//Validates the platform topology at startup and displays an unhealthy state if no children are defined.
//This also means the dashboard shows the platform as Starting with an error message instead of Running with missing children, which is more intuitive for users and easier to troubleshoot.
//Adjusts the platform resource state based on children - if all running then platform is healthy, if any stopped or none present, then platform is unhealthy.
sealed class PlatformTopologyEventingSubscriber(
    ResourceNotificationService notifications,
    PlatformReadinessState readinessState,
    ILogger<PlatformTopologyEventingSubscriber> logger)
    : IDistributedApplicationEventingSubscriber
{
    public Task SubscribeAsync(
        IDistributedApplicationEventing eventing,
        DistributedApplicationExecutionContext executionContext,
        CancellationToken cancellationToken = default)
    {
        eventing.Subscribe<BeforeStartEvent>(OnBeforeStart);
        eventing.Subscribe<ResourceReadyEvent>(OnResourceReady);
        eventing.Subscribe<ResourceStoppedEvent>(OnResourceStopped);
        return Task.CompletedTask;
    }

    // BeforeStartEvent fires after all resources are registered but before the orchestrator launches any.
    // DistributedApplicationModel is only reachable via evt.Services (the app DI container), not on the event itself.
    async Task OnBeforeStart(BeforeStartEvent evt, CancellationToken cancellationToken)
    {
        var model = evt.Model;
        foreach (var platform in model.Resources.OfType<ParticularPlatformResource>())
        {
            await Configure(model, platform, cancellationToken).ConfigureAwait(false);
        }
    }

    async Task Configure(DistributedApplicationModel model, ParticularPlatformResource platform, CancellationToken cancellationToken)
    {
        var children = FindChildren(model, platform).ToList();

        await ValidateTopology(platform, children, cancellationToken).ConfigureAwait(false);
        ValidateContainerImageVersionAlignment(platform, children);

        readinessState.Register(platform, children.Count);
    }

    async Task ValidateTopology(ParticularPlatformResource platform, IReadOnlyList<IResource> children, CancellationToken cancellationToken)
    {
        if (children.Count == 0)
        {
            logger.LogInformation(
                "Platform '{Platform}' Unhealthy — no child resources added.",
                platform.Name);

            await notifications.PublishUpdateAsync(platform, s => s with
            {
                State = new ResourceStateSnapshot(KnownResourceStates.RuntimeUnhealthy, KnownResourceStateStyles.Error)
            }).ConfigureAwait(false);

            //NOTE once the InteractionService https://aspire.dev/extensibility/interaction-service/ comes out of preview, we should add a user-friendly message here to guide them on next steps (e.g. "Add child resources to the platform to resolve this issue. Click here to learn how.").
        }
    }

    // ResourceReadyEvent fires once per resource when it transitions to Running + all health checks pass.
    // Mark the child in the tracker; when the last one flips, PublishUpdateAsync moves the synthetic
    // platform resource to Running so consumers waiting on .WaitFor(platform) unblock.
    async Task OnResourceReady(ResourceReadyEvent evt, CancellationToken cancellationToken)
    {
        var platform = FindParentPlatform(evt.Resource);
        if (platform is null)
        {
            return;
        }

        if (readinessState.MarkReady(platform, evt.Resource.Name))
        {
            logger.LogInformation(
                "Platform '{Platform}' reached Running — all child resources are ready.",
                platform.Name);

            await notifications.PublishUpdateAsync(platform, s => s with
            {
                State = new ResourceStateSnapshot(KnownResourceStates.Running, KnownResourceStateStyles.Success)
            }).ConfigureAwait(false);
        }
    }

    // ResourceStoppedEvent fires once per resource when it transitions to Stopped.
    // Mark the child in the tracker as stopped; if any are stopped, the platform is Unhealthy since it can't function without all its defined children.
    async Task OnResourceStopped(ResourceStoppedEvent evt, CancellationToken cancellationToken)
    {
        var platform = FindParentPlatform(evt.Resource);
        if (platform is null)
        {
            return;
        }

        if (readinessState.MarkStopped(platform, evt.Resource.Name))
        {
            logger.LogInformation(
                "Platform '{Platform}' reached Unhealthy — a child resource has stopped.",
                platform.Name);

            await notifications.PublishUpdateAsync(platform, s => s with
            {
                State = new ResourceStateSnapshot(KnownResourceStates.RuntimeUnhealthy, KnownResourceStateStyles.Warn)
            }).ConfigureAwait(false);
        }
    }

    void ValidateContainerImageVersionAlignment(ParticularPlatformResource platform, IReadOnlyList<IResource> children)
    {
        var serviceControlVersions = children
            .Where(c => c is ServiceControlErrorInstanceResource or ServiceControlAuditInstanceResource or ServiceControlMonitoringInstanceResource)
            .Select(c => (Resource: c, Annotation: c.TryGetLastAnnotation<ContainerImageAnnotation>(out var a) ? a : null))
            .Where(c => c.Annotation is not null)
            .Select(c => (c.Resource, c.Annotation!.Image, Tag: c.Annotation.Tag ?? "latest"))
            .ToList();

        if (serviceControlVersions.Count < 2)
        {
            return;
        }

        var distinctTags = serviceControlVersions.Select(v => v.Tag).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (distinctTags.Count > 1)
        {
            var details = string.Join(", ", serviceControlVersions.Select(v => $"{v.Resource.Name} ({v.Image}:{v.Tag})"));
            logger.LogWarning(
                "Platform '{Platform}' has mismatched ServiceControl container image versions: {Details}. " +
                "All ServiceControl components should use the same version to ensure compatibility.",
                platform.Name, details);
        }
    }

    // IResourceWithParent<T> is Aspire's typed parent/child marker: the Parent property is populated at
    // construction time and gives dashboard nesting + lifecycle coupling. We use it purely for discovery
    // here — no annotation walk required.
    static ParticularPlatformResource? FindParentPlatform(IResource resource) =>
        (resource as IResourceWithParent<ParticularPlatformResource>)?.Parent;

    static IEnumerable<IResource> FindChildren(DistributedApplicationModel model, ParticularPlatformResource platform)
        => model.Resources
            .Where(r => r is IResourceWithParent<ParticularPlatformResource> rwp && rwp.Parent == platform);


}
