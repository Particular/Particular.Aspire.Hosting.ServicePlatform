namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Persistence;
using TestResources;
using ThroughputReporting;
using Transport;

public class PublishPlatformDefaultsWithSqsThroughputReportingApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var accessKeyId = builder.AddParameter("sqs-access-key-id");
        var secretAccessKey = builder.AddParameter("sqs-secret-access-key", secret: true);
        var region = builder.AddParameter("sqs-region");
        var queueNamePrefix = builder.AddParameter("sqs-queue-name-prefix");
        var topicNamePrefix = builder.AddParameter("sqs-topic-name-prefix");
        var s3BucketForLargeMessages = builder.AddParameter("sqs-s3-bucket-for-large-messages");
        var s3KeyPrefix = builder.AddParameter("sqs-s3-key-prefix");
        var doNotWrapOutgoingMessages = builder.AddParameter("sqs-do-not-wrap-outgoing-messages");
        var reservedBytesInMessageSize = builder.AddParameter("sqs-reserved-bytes-in-message-size");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAmazonSqs(builder.AddDummyConnectionString("transport-connection"));

        var persistence = platform.AddPersistenceRavenDb("particular-persistence");

        platform
            .AddServiceControlErrorInstance("particular-error", persistence)
            .WithThroughputQueue("particular.throughput")
            .WithThroughputReporting(new ThroughputReportingAmazonSqs(
                ReferenceExpression.Create($"{accessKeyId}"),
                ReferenceExpression.Create($"{secretAccessKey}"),
                ReferenceExpression.Create($"{region}"),
                ReferenceExpression.Create($"{queueNamePrefix}"),
                ReferenceExpression.Create($"{topicNamePrefix}"),
                ReferenceExpression.Create($"{s3BucketForLargeMessages}"),
                ReferenceExpression.Create($"{s3KeyPrefix}"),
                ReferenceExpression.Create($"{doNotWrapOutgoingMessages}"),
                ReferenceExpression.Create($"{reservedBytesInMessageSize}")));

        platform.AddDefaultComponents();
    }
}
