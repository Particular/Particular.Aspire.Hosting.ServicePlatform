namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using System.Threading;
using System.Threading.Tasks;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Tests;

public class PublishPlatformDefaultsAmazonSqsAdaptedValueApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");
        var accesskey = builder.AddParameter("accesskey", "access-key-value", secret: true);
        var secretKey = builder.AddParameter("secretKey", "secret-key-value", secret: true);
        var bucket = builder.AddParameter("bucket", "my-bucket-value", secret: true);

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAmazonSqs(
                "us-east-1",
                accesskey.Resource,
                secretKey.Resource,
                "transport-prefix",
                s3BucketForLargeMessages: new AWSOutputReference(bucket.Resource));

        platform.AddDefaultComponents();
    }

    /// <summary>
    /// At the time of writing this test the Aspire.Hosting.AWS package exposes it's output values without the IExpressionValue interface
    /// to avoid taking a reference on the AWS code this case is handled by an adapter that adds the combined interface onto the passed in type.
    /// This type is here to emulate that behavior.
    /// </summary>
    /// <param name="inner"></param>
    class AWSOutputReference(IExpressionValue inner) : IValueProvider, IManifestExpressionProvider
    {
        public ValueTask<string?> GetValueAsync(CancellationToken cancellationToken = new()) => inner.GetValueAsync(cancellationToken);

        public string ValueExpression => inner.ValueExpression;
    }
}


