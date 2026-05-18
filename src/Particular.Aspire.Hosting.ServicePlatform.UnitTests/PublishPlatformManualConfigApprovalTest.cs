namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using Particular.Aspire.Hosting.ServicePlatform.Persistence;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

public class PublishPlatformManualConfigApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("docker-compose");
        var param = builder.AddDummyConnectionString("transport-connection");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAzureServiceBus(param);

        var persistence = platform.AddPersistenceRavenDb("particular-persistence");
        var servicecontrol = platform.AddServiceControlErrorInstance("particular-error", persistence);
        var monitoring = platform.AddServiceControlMonitoringInstance("particular-monitoring");
        platform.AddServicePulse("particular-servicepulse", servicecontrol, monitoring);
    }
}