namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using TestResources;

public class PublishPlatformDefaultsSqlServerTransportApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportSqlServer(
                builder.AddDummyConnectionString("transport-connection"),
                queueSchema: "custom-queue-schema",
                subscriptionsTable: "custom-subscriptions-table")
            .AddDefaultComponents();

        builder.AddContainer("endpoint", "endpoint_image")
            .WithParticularPlatform(platform);
    }
}
