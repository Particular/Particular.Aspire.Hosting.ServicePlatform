namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Controls how a ServiceControl instance container starts: whether it performs setup
/// (e.g. creating queues), runs the instance, or both.
/// </summary>
public enum PlatformRunMode
{
    /// <summary>
    /// Run setup and then run the instance. This is the default and emits the
    /// <c>--setup-and-run</c> container argument.
    /// </summary>
    SetupAndRun,

    /// <summary>
    /// Run the instance without performing setup, assuming queues and database structures already
    /// exist. Emits no run-mode container argument.
    /// </summary>
    Run,

    /// <summary>
    /// Perform setup only and then exit, without running the instance. Emits the <c>--setup</c>
    /// container argument.
    /// </summary>
    Setup,
}
