namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Platform;

class AmazonSqsTransportAnnotation(
    IExpressionValue region,
    IExpressionValue accessKeyId,
    IExpressionValue secretAccessKey,
    IExpressionValue? queueNamePrefix,
    IExpressionValue? topicNamePrefix,
    IExpressionValue? s3BucketForLargeMessages,
    IExpressionValue? s3KeyPrefix,
    IExpressionValue? doNotWrapOutgoingMessages,
    IExpressionValue? reservedBytesInMessageSize
) : IPlatformTransportAnnotation
{
    ReferenceExpression CreateConnectionString()
    {
        var builder = new ReferenceExpressionBuilder();
        builder.Append($"Region={region};AccessKeyId={accessKeyId};SecretAccessKey={secretAccessKey};");
        if (queueNamePrefix != null)
        {
            builder.Append($"QueueNamePrefix={queueNamePrefix};");
        }

        if (topicNamePrefix != null)
        {
            builder.Append($"TopicNamePrefix={topicNamePrefix};");
        }

        if (s3BucketForLargeMessages != null)
        {
            builder.Append($"S3BucketForLargeMessages={s3BucketForLargeMessages};");
        }

        if (s3KeyPrefix != null)
        {
            builder.Append($"S3KeyPrefix={s3KeyPrefix};");
        }

        if (doNotWrapOutgoingMessages != null)
        {
            builder.Append($"DoNotWrapOutgoingMessages={doNotWrapOutgoingMessages};");
        }

        if (reservedBytesInMessageSize != null)
        {
            builder.Append($"ReservedBytesInMessageSize={reservedBytesInMessageSize};");
        }

        return builder.Build();
    }


    public void ApplyTo<T>(IResourceBuilder<T> resource) where T : IResourceWithEnvironment
    {
        if (resource is IResourceBuilder<IPlatformComponent>)
        {
            resource.WithEnvironment(context =>
            {
                context.EnvironmentVariables[PlatformEnvironment.ServiceControl.TransportType] = "AmazonSQS";
                context.EnvironmentVariables[PlatformEnvironment.ServiceControl.ConnectionString] = CreateConnectionString();
            });
        }

        // deliberately not expanding properties onto endpoints, as SQL Connection Strings are not a canonical thing
    }
}