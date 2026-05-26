namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using global::Aspire.Hosting;

public class PublishPlatformWithoutSetupApprovalTest : AspireApplicationPublishingTestBase
{
    // Mirrors the default-components topology, but disables setup on each ServiceControl instance so the
    // published container commands omit the "--setup-and-run" argument.
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportLearning();

        var persistence = platform.AddPersistenceRavenDb("particular-persistence");

        var error = platform
            .AddServiceControlErrorInstance("particular-error", persistence)
            .WithoutSetup();

        var monitoring = platform
            .AddServiceControlMonitoringInstance("particular-monitoring")
            .WithoutSetup();

        platform
            .AddServiceControlAuditInstance("particular-audit", error, persistence)
            .WithoutSetup();

        platform.AddServicePulse("particular-servicepulse", error, monitoring);
    }
}
