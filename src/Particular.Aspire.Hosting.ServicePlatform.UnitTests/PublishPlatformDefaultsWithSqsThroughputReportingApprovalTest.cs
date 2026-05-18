namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Persistence;
using Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

public class PublishPlatformDefaultsWithSqsThroughputReportingApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAmazonSqs(builder.AddDummyConnectionString("transport-connection"));

        var persistence = platform.AddPersistenceRavenDb("particular-persistence");

        platform
            .AddServiceControlErrorInstance("particular-error", persistence)
            .WithThroughputQueue("particular.throughput")
            .WithThroughputReporting(new ThroughputReportingAmazonSqs(
                ReferenceExpression.Create($"~~access-key~~"),
                ReferenceExpression.Create($"~~secret-key~~"),
                ReferenceExpression.Create($"~~region~~"),
                ReferenceExpression.Create($"~~queueNamePrefix~~"),
                ReferenceExpression.Create($"~~topicNamePrefix~~")));

        platform.AddDefaultComponents();
    }
}
