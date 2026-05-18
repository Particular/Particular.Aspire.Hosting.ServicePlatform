namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

public class PublishPlatformDefaultsWithSqsTransportApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        var sampleConnectionString =
            "Region=<REGION>;QueueNamePrefix=<prefix>;TopicNamePrefix=<prefix>;AccessKeyId=<ACCESSKEYID>;" +
            "SecretAccessKey=<SECRETACCESSKEY>;S3BucketForLargeMessages=<BUCKETNAME>;S3KeyPrefix=<KEYPREFIX>";

        builder.AddDockerComposeEnvironment("compose");

        builder
            .AddParticularPlatform("particular")
            .WithTransportAmazonSqs(builder.AddDummyConnectionString("transport-connection", sampleConnectionString))
            .AddDefaultComponents();
    }
}