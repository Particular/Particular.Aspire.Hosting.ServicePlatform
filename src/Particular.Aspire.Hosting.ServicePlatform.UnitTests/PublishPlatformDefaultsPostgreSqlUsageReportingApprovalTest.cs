namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Persistence;
using Tests;
using Tests.TestResources;
using ThroughputReporting;
using Transport;

public class PublishPlatformDefaultsPostgreSqlUsageReportingApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportPostgreSql(builder.AddDummyConnectionString("transport-connection"));

        platform
            .AddServiceControlErrorInstance("particular-error", platform.AddPersistenceRavenDb("particular-persistence"))
            .WithThroughputReporting(new ThroughputReportingPostgreSql(
                builder.AddDummyConnectionString("throughput-connection", "connection-string-resource").Resource
            ));

        platform.AddDefaultComponents();
    }
}
