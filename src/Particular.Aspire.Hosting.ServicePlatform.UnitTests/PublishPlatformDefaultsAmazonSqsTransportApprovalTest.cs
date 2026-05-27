namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Tests;

public class PublishPlatformDefaultsAmazonSqsTransportApprovalTest : AspireApplicationPublishingTestBase
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
                    conf.S3BucketForLargeMessages = ReferenceExpression.Create($"stringBucket");
                });

        platform.AddDefaultComponents();
    }
}


