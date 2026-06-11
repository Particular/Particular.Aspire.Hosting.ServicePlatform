namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using TestResources;

public class PublishPlatformDefaultsSqlServerTransportApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        builder
            .AddParticularPlatform("particular")
            .WithTransportSqlServer(
                builder.AddDummyConnectionString("transport-connection"),
                queueSchema: "custom-queue-schema",
                subscriptionsTable: "custom-subscriptions-table")
            .AddDefaultComponents();
    }
}
