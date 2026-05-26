namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using global::Aspire.Hosting.ApplicationModel;

// Holds the configured run mode for a ServiceControl instance. The value is read when the args
// callback runs at build/publish time, so it is order-independent relative to WithRunMode().
sealed class RunModeAnnotation(PlatformRunMode mode) : IResourceAnnotation
{
    public PlatformRunMode Mode { get; } = mode;
}
