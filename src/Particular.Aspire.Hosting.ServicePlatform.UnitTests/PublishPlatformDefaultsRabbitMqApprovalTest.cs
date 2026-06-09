namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using NUnit.Framework;
using TestResources;
using Transport;

[TestFixture(RabbitMqRouting.QuorumConventionalRouting)]
[TestFixture(RabbitMqRouting.ClassicConventionalRouting)]
[TestFixture(RabbitMqRouting.QuorumDirectRouting)]
[TestFixture(RabbitMqRouting.ClassicDirectRouting)]
public class PublishPlatformDefaultsRabbitMqApprovalTest(RabbitMqRouting routing) : AspireApplicationPublishingTestBase
{
    protected override string? Scenario => routing.ToString();

    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportRabbitMQ(routing, builder.AddDummyConnectionString("transport-connection"));

        platform.AddServiceControlErrorInstance("particular-error", platform.AddPersistenceRavenDb("particular-persistence"));

        platform.AddDefaultComponents();
    }
}
