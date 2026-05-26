namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using global::Aspire.Hosting.ApplicationModel;

// Marks a ServiceControl instance to start in run-only mode, suppressing the "--setup-and-run"
// container argument. Presence of this annotation is checked when the args callback runs at
// build/publish time, so it is order-independent relative to WithoutSetup().
sealed class SkipSetupAnnotation : IResourceAnnotation;
