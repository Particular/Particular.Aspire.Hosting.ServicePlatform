namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Persistence;
using Tests;
using Tests.TestResources;
using ThroughputReporting;
using Transport;

public class PublishPlatformDefaultsSqlServerUsageReportingApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportSqlServer(builder.AddDummyConnectionString("transport-connection"));

        platform
            .AddServiceControlErrorInstance("particular-error", platform.AddPersistenceRavenDb("particular-persistence"))
            .WithThroughputReporting(new ThroughputReportingSqlServer(
                builder.AddDummyConnectionString("throughput-connection", "connection-string-resource").Resource,
                ReferenceExpression.Create($"additional-catalogs")
            ));

        platform.AddDefaultComponents();
    }
}
