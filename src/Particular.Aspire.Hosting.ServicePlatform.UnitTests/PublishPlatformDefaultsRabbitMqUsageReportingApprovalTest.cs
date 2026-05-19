namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Persistence;
using Tests;
using Tests.TestResources;
using ThroughputReporting;
using Transport;

public class PublishPlatformDefaultsRabbitMqUsageReportingApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportRabbitMq(RabbitMqRouting.ClassicDirectRouting, builder.AddDummyConnectionString("transport-connection"));

        platform
            .AddServiceControlErrorInstance("particular-error", platform.AddPersistenceRavenDb("particular-persistence"))
            .WithThroughputReporting(new ThroughputReportingRabbitMq(
                builder.AddDummyConnectionString("throughput-connection", "connection-string-resource").Resource,
                ReferenceExpression.Create($"ref-expression"),
                builder.AddParameter("param", "parameter-value").Resource
            ));

        platform.AddDefaultComponents();
    }
}