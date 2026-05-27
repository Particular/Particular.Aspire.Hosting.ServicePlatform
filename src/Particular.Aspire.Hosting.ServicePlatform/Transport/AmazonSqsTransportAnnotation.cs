namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Platform;

class AmazonSqsTransportAnnotation(AmazonSqsTransportSettings settings) : IPlatformTransportAnnotation
{
    public AmazonSqsTransportSettings Settings { get; } = settings;

    ReferenceExpression CreateConnectionString()
    {
        var builder = new ReferenceExpressionBuilder();
        builder.Append($"Region={Settings.Region};AccessKeyId={Settings.AccessKeyId};SecretAccessKey={Settings.SecretAccessKey};");
        if (Settings.QueueNamePrefix != null)
        {
            builder.AppendLiteral($"QueueNamePrefix={Settings.QueueNamePrefix};");
        }

        if (Settings.TopicNamePrefix != null)
        {
            builder.AppendLiteral($"TopicNamePrefix={Settings.TopicNamePrefix};");
        }

        if (Settings.S3BucketForLargeMessages != null)
        {
            builder.Append($"S3BucketForLargeMessages={Settings.S3BucketForLargeMessages};");
        }

        if (Settings.S3KeyPrefix != null)
        {
            builder.AppendLiteral($"S3KeyPrefix={Settings.S3KeyPrefix};");
        }

        if (Settings.DoNotWrapOutgoingMessages != null)
        {
            builder.AppendLiteral($"DoNotWrapOutgoingMessages={Settings.DoNotWrapOutgoingMessages};");
        }

        if (Settings.ReservedBytesInMessageSize != null)
        {
            builder.AppendLiteral($"ReservedBytesInMessageSize={Settings.ReservedBytesInMessageSize};");
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
            return;
        }

        resource.WithEnvironment("AWS_REGION", Settings.Region);
        resource.WithEnvironment("AWS_ACCESS_KEY_ID", Settings.AccessKeyId);
        resource.WithEnvironment("AWS_SECRET_ACCESS_KEY", Settings.SecretAccessKey);
    }
}