namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// Particular Platform transport configuration for AmazonSQS
/// See
///  * https://docs.particular.net/servicecontrol/transports#amazon-sqs
///  * https://docs.particular.net/transports/sqs/configuration-options#region
/// </summary>
public class AmazonSqsTransportSettings
{
    /// <summary>
    /// The AWS region.
    /// </summary>
    public required string Region { get; init; }

    /// <summary>
    /// The AWS access key ID.
    /// </summary>
    public required IExpressionValue AccessKeyId { get; init; }

    /// <summary>
    /// The AWS secret access key.
    /// </summary>
    public required IExpressionValue SecretAccessKey { get; init; }

    /// <summary>
    /// Optional queue name prefix.
    /// </summary>

    public string? QueueNamePrefix { get; set; }

    /// <summary>
    /// Optional topic name prefix.
    /// </summary>
    public string? TopicNamePrefix { get; set; }

    /// <summary>
    /// Optional S3 bucket for large message payloads, can be provided from a cloud formation output
    /// </summary>
    public IExpressionValue? S3BucketForLargeMessages { get; set; }

    /// <summary>
    /// Optional S3 key prefix used with large messages.
    /// </summary>
    public string? S3KeyPrefix { get; set; }

    /// <summary>
    /// Optional value to control message wrapping.
    /// </summary>
    public bool? DoNotWrapOutgoingMessages { get; set; }

    /// <summary>
    /// Optional reserved bytes setting for message size calculations.
    /// </summary>
    public int? ReservedBytesInMessageSize { get; set; }
}