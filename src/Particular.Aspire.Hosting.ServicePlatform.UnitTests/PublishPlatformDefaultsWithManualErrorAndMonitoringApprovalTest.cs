namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using global::Aspire.Hosting;
using Particular.Aspire.Hosting.ServicePlatform.Tests.TestResources;

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
