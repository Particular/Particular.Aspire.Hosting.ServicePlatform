namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using Tests;
using Tests.TestResources;
using Transport;

public class PublishPlatformDefaultsRabbitMqApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportRabbitMq(RabbitMqRouting.ClassicDirectRouting, builder.AddDummyConnectionString("transport-connection"));

        platform.AddServiceControlErrorInstance("particular-error", platform.AddPersistenceRavenDb("particular-persistence"));

        platform.AddDefaultComponents();
    }
}