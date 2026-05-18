namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using System;
using Aspire.ServicePlatform.Platform;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Platform;

public sealed class ThroughputReportingAmazonSqs : IThroughputReportingProvider
{
    internal const string AccessKeyIdEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_ACCESSKEYID";
    internal const string SecretAccessKeyEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_SECRETACCESSKEY";
    internal const string RegionEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_REGION";
    internal const string QueueNamePrefixEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_QUEUENAMEPREFIX";
    internal const string TopicNamePrefixEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_TOPICNAMEPREFIX";
    internal const string S3BucketForLargeMessagesEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_S3BUCKETFORLARGEMESSAGES";
    internal const string S3KeyPrefixEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_S3KEYPREFIX";
    internal const string DoNotWrapOutgoingMessagesEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_DONOTWRAPOUTGOINGMESSAGES";
    internal const string ReservedBytesInMessageSizeEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_RESERVEDBYTESINMESSAGESIZE";

    readonly ReferenceExpression? accessKeyId;
    readonly ReferenceExpression? secretAccessKey;
    readonly ReferenceExpression? region;
    readonly ReferenceExpression? queueNamePrefix;
    readonly ReferenceExpression? topicNamePrefix;
    readonly ReferenceExpression? s3BucketForLargeMessages;
    readonly ReferenceExpression? s3KeyPrefix;
    readonly ReferenceExpression? doNotWrapOutgoingMessages;
    readonly ReferenceExpression? reservedBytesInMessageSize;

    public ThroughputReportingAmazonSqs(
        ReferenceExpression? accessKeyId = null,
        ReferenceExpression? secretAccessKey = null,
        ReferenceExpression? region = null,
        ReferenceExpression? queueNamePrefix = null,
        ReferenceExpression? topicNamePrefix = null,
        ReferenceExpression? s3BucketForLargeMessages = null,
        ReferenceExpression? s3KeyPrefix = null,
        ReferenceExpression? doNotWrapOutgoingMessages = null,
        ReferenceExpression? reservedBytesInMessageSize = null)
    {
        ArgumentNullException.ThrowIfNull(accessKeyId);
        ArgumentNullException.ThrowIfNull(secretAccessKey);
        ArgumentNullException.ThrowIfNull(region);

        this.accessKeyId = accessKeyId;
        this.secretAccessKey = secretAccessKey;
        this.region = region;
        this.queueNamePrefix = queueNamePrefix;
        this.topicNamePrefix = topicNamePrefix;
        this.s3BucketForLargeMessages = s3BucketForLargeMessages;
        this.s3KeyPrefix = s3KeyPrefix;
        this.doNotWrapOutgoingMessages = doNotWrapOutgoingMessages;
        this.reservedBytesInMessageSize = reservedBytesInMessageSize;
    }

    public void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        ArgumentNullException.ThrowIfNull(errorInstance);

        if (errorInstance.Resource.Parent.GetTransport() is not AmazonSqsTransportAnnotation)
        {
            throw new InvalidOperationException(
                $"{nameof(ThroughputReportingAmazonSqs)} requires the parent platform to be configured with WithTransportAmazonSqs first.");
        }

        if (accessKeyId != null)
        {
            errorInstance.WithEnvironment(AccessKeyIdEnvVar, accessKeyId);
        }

        if (secretAccessKey != null)
        {
            errorInstance.WithEnvironment(SecretAccessKeyEnvVar, secretAccessKey);
        }

        if (region != null)
        {
            errorInstance.WithEnvironment(RegionEnvVar, region);
        }

        if (queueNamePrefix is not null)
        {
            errorInstance.WithEnvironment(QueueNamePrefixEnvVar, queueNamePrefix);
        }

        if (topicNamePrefix is not null)
        {
            errorInstance.WithEnvironment(TopicNamePrefixEnvVar, topicNamePrefix);
        }

        if (s3BucketForLargeMessages is not null)
        {
            errorInstance.WithEnvironment(S3BucketForLargeMessagesEnvVar, s3BucketForLargeMessages);
        }

        if (s3KeyPrefix is not null)
        {
            errorInstance.WithEnvironment(S3KeyPrefixEnvVar, s3KeyPrefix);
        }

        if (doNotWrapOutgoingMessages is not null)
        {
            errorInstance.WithEnvironment(DoNotWrapOutgoingMessagesEnvVar, doNotWrapOutgoingMessages);
        }

        if (reservedBytesInMessageSize is not null)
        {
            errorInstance.WithEnvironment(ReservedBytesInMessageSizeEnvVar, reservedBytesInMessageSize);
        }
    }
}
