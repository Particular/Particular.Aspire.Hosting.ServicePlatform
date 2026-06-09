namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using TestResources;

public class PublishPlatformWithCustomEndpointPortsApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAzureServiceBus(builder.AddDummyConnectionString("transport-connection"));

        var persistence = platform.AddPersistenceRavenDb("particular-persistence");

        var error = platform.AddServiceControlErrorInstance("particular-error", persistence)
            .WithEndpoint("http", e => { e.Port = 13333; e.IsExternal = true; });

        platform.AddServiceControlAuditInstance("particular-audit", error, persistence)
            .WithEndpoint("http", e => { e.Port = 14444; e.IsExternal = true; });

        var monitoring = platform.AddServiceControlMonitoringInstance("particular-monitoring")
            .WithEndpoint("http", e => { e.Port = 13633; e.IsExternal = true; });

        platform.AddServicePulse("particular-servicepulse", error, monitoring)
            .WithEndpoint("http", e => { e.Port = 19090; e.IsExternal = true; });
    }
}
