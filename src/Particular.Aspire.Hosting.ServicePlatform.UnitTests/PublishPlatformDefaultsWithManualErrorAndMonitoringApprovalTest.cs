namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using TestResources;

public class PublishPlatformDefaultsWithManualErrorAndMonitoringApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAzureServiceBus(builder.AddDummyConnectionString("transport-connection"));

        var persistence = platform.AddPersistenceRavenDb("custom-persistence");
        platform.AddServiceControlErrorInstance("custom-error", persistence);
        platform.AddServiceControlMonitoringInstance("custom-monitoring");

        platform.AddDefaultComponents();
    }
}
