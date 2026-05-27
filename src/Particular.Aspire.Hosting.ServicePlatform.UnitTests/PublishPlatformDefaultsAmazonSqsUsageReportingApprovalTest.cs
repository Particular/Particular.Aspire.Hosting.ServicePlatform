namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Tests;
using Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

public class PublishPlatformDefaultsAmazonSqsUsageReportingApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");
        var accesskey = builder.AddParameter("accesskey", "access-key-value", secret: true);
        var secretKey = builder.AddParameter("secretKey", "secret-key-value", secret: true);

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAmazonSqs(
                "us-east-1",
                accesskey.Resource,
                secretKey.Resource, conf =>
                {
                    conf.QueueNamePrefix = "transport-prefix";
                });

        platform
            .AddServiceControlErrorInstance("particular-error", platform.AddPersistenceRavenDb("particular-persistence"))
            .WithThroughputReporting(new ThroughputReportingAmazonSqs(
                accesskey.Resource,
                secretKey.Resource,
                ReferenceExpression.Create($"throughput-profile"),
                ReferenceExpression.Create($"throughput-region"),
                ReferenceExpression.Create($"throughput-prefix")
            ));

        platform.AddDefaultComponents();
    }
}


