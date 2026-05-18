namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using global::Aspire.Hosting.ApplicationModel;

// Marks an error instance as having throughput reporting configured and records which provider supplied it,
// so other parts of the model (validators, publish hooks, tests) can introspect the wire-up without re-reading env vars.
sealed class ThroughputReportingAnnotation(IThroughputReportingProvider provider) : IResourceAnnotation
{
    public IThroughputReportingProvider Provider { get; } = provider;
}
