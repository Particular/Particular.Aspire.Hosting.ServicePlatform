namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using TestResources;

public class PublishPlatformWithCustomQueueNamesApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAzureServiceBus(builder.AddDummyConnectionString("transport-connection"));

        var persistence = platform.AddPersistenceRavenDb("particular-persistence");

        var error = platform.AddServiceControlErrorInstance("particular-error", persistence)
            .WithErrorQueueName("custom-error-queue")
            .WithThroughputQueue("custom-throughput-queue");

        platform.AddServiceControlAuditInstance("particular-audit", error, persistence)
            .WithAuditQueueName("custom-audit-queue");

        platform.AddDefaultComponents();
    }
}
