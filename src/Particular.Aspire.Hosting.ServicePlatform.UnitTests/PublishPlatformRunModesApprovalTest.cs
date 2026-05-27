namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using global::Aspire.Hosting;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public class PublishPlatformRunModesApprovalTest : AspireApplicationPublishingTestBase
{
    // Exercises each PlatformRunMode so the published container commands reflect the mapping:
    // Setup -> "--setup", Run -> (no command), and the default when WithRunMode is not
    // called -> "--setup-and-run".
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportLearning();

        var persistence = platform.AddPersistenceRavenDb("particular-persistence");

        var error = platform
            .AddServiceControlErrorInstance("particular-error", persistence)
            .WithRunMode(PlatformRunMode.Setup);

        var monitoring = platform
            .AddServiceControlMonitoringInstance("particular-monitoring")
            .WithRunMode(PlatformRunMode.Run);

        // No WithRunMode call: exercises the default, which must emit "--setup-and-run".
        platform.AddServiceControlAuditInstance("particular-audit", error, persistence);

        platform.AddServicePulse("particular-servicepulse", error, monitoring);
    }
}
