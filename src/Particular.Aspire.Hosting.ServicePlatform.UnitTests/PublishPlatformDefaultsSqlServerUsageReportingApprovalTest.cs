namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using ThroughputReporting;
using TestResources;

public class PublishPlatformDefaultsSqlServerUsageReportingApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportSqlServer(
                builder.AddDummyConnectionString("transport-connection"),
                queueSchema: "custom-queue-schema",
                subscriptionsTable: "custom-subscriptions-table");

        platform
            .AddServiceControlErrorInstance("particular-error", platform.AddPersistenceRavenDb("particular-persistence"))
            .WithThroughputReporting(new ThroughputReportingSqlServer(additionalCatalogs: "catalog-a,catalog-b"));

        platform.AddDefaultComponents();
    }
}
