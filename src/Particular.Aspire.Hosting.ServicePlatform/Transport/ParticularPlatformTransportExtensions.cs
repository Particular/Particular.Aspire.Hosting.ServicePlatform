//extension methods should be in the Aspire hosting namespace as per https://github.com/Particular/Particular.Aspire.Hosting.ServicePlatform/blob/main/docs/aspire-integration-guide.md#naming-conventions
namespace Aspire.Hosting;

using Particular.Aspire.Hosting.ServicePlatform.Platform;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Threading;
using System.Threading.Tasks;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

/// <summary>
/// Extension methods for configuring the message transport for the Particular Service Platform.
/// </summary>
public static class ParticularPlatformTransportExtensions
{
    extension(IResourceBuilder<ParticularPlatformResource> platform)
    {
        /// <summary>
        /// Configures the platform to use the Learning transport, which stores messages on the local file system.
        /// This transport is intended for development and testing only and is not supported in publish mode
        /// unless explicitly enabled via the <c>Particular:AllowLearningTransportPublish</c> configuration setting.
        /// </summary>
        /// <param name="storagePath">The file system path for message storage. Defaults to <c>.learningtransport</c> in the current directory.</param>
        /// <returns>The platform resource builder for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if used in publish mode without enabling <c>Particular:AllowLearningTransportPublish</c>.</exception>
        public IResourceBuilder<ParticularPlatformResource> WithTransportLearning(string? storagePath = null)
        {
            storagePath ??= ".learningtransport";
            var resolvedPath = Path.IsPathRooted(storagePath)
                ? storagePath
                : Path.GetFullPath(storagePath, platform.ApplicationBuilder.AppHostDirectory);

            Directory.CreateDirectory(resolvedPath);

            var transportConnection = platform.ApplicationBuilder
                .AddConnectionString($"learning-transport", ReferenceExpression.Create($"{resolvedPath}"))
                .WithParentRelationship(platform);

            platform.WithAnnotation(new LearningTransportAnnotation(resolvedPath, transportConnection.Resource));

            if (platform.ApplicationBuilder.ExecutionContext.IsPublishMode)
            {
                var allowLearningPublish = platform.ApplicationBuilder.Configuration.GetValue(
                        LearningTransportAnnotation.SettingsEnablePublish, false
                    );
                if (!allowLearningPublish)
                {
                    throw new InvalidOperationException(
                        "The LearningTransport is not supported in publish mode.");
                }
            }

            return platform;
        }

        /// <summary>
        /// Configures the platform to use Azure Service Bus as the message transport.
        /// </summary>
        /// <param name="azureServiceBus">The Azure Service Bus resource providing the connection string.</param>
        /// <returns>The platform resource builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="azureServiceBus"/> is null.</exception>
        public IResourceBuilder<ParticularPlatformResource> WithTransportAzureServiceBus(
            IResourceBuilder<IResourceWithConnectionString> azureServiceBus)
        {
            ArgumentNullException.ThrowIfNull(azureServiceBus);
            return platform.WithAnnotation(new AzureServiceBusTransportAnnotation(azureServiceBus.Resource));
        }

        /// <summary>
        /// Configures the platform to use RabbitMQ as the message transport.
        /// </summary>
        /// <param name="routingType">The type of routing to use.</param>
        /// <param name="rabbitMQ">The RabbitMQ resource providing the connection string.</param>
        /// <returns>The platform resource builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="rabbitMQ"/> is null.</exception>
        public IResourceBuilder<ParticularPlatformResource> WithTransportRabbitMQ(
            RabbitMqRouting routingType,
            IResourceBuilder<IResourceWithConnectionString> rabbitMQ)
        {
            ArgumentNullException.ThrowIfNull(rabbitMQ);
            return platform.WithAnnotation(new RabbitMqTransportAnnotation(routingType, rabbitMQ.Resource));
        }

        /// <summary>
        /// Configures the platform to use Amazon SQS as the message transport.
        /// </summary>
        /// <param name="region">The AWS region.</param>
        /// <param name="accessKeyId">The AWS access key ID.</param>
        /// <param name="secretAccessKey">The AWS secret access key.</param>
        /// <param name="queueNamePrefix">Optional queue name prefix.</param>
        /// <param name="topicNamePrefix">Optional topic name prefix.</param>
        /// <param name="s3BucketForLargeMessages">Optional S3 bucket for large message payloads, can be provided from a cloud formation output</param>
        /// <param name="s3KeyPrefix">Optional S3 key prefix used with large messages.</param>
        /// <param name="doNotWrapOutgoingMessages">Optional value to control message wrapping.</param>
        /// <param name="reservedBytesInMessageSize">Optional reserved bytes setting for message size calculations.</param>
        /// <returns>The platform resource builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if required values are null.</exception>
        public IResourceBuilder<ParticularPlatformResource> WithTransportAmazonSqs(
            string region,
            IExpressionValue accessKeyId,
            IExpressionValue secretAccessKey,
            string? queueNamePrefix = null,
            string? topicNamePrefix = null,
            object? s3BucketForLargeMessages = null,
            string? s3KeyPrefix = null,
            bool? doNotWrapOutgoingMessages = null,
            int? reservedBytesInMessageSize = null)
        {
            ArgumentNullException.ThrowIfNull(region);
            ArgumentNullException.ThrowIfNull(accessKeyId);
            ArgumentNullException.ThrowIfNull(secretAccessKey);

            var s3BucketExpression = s3BucketForLargeMessages switch
            {
                null => null,
                string s => ReferenceExpression.Create($"{s}"),
                IExpressionValue ev => ev,
                IValueProvider and IManifestExpressionProvider x => new ExpressionValueAdapter(x),
                _ => throw new ArgumentException("S3 bucket must be either a string or an expression value", nameof(s3BucketForLargeMessages)),
            };

            return platform.WithAnnotation(new AmazonSqsTransportAnnotation(
                region,
                accessKeyId,
                secretAccessKey,
                queueNamePrefix,
                topicNamePrefix,
                s3BucketExpression,
                s3KeyPrefix,
                doNotWrapOutgoingMessages,
                reservedBytesInMessageSize));
        }
    }

    /// <summary>
    /// Allows classes that implement both IValueProvider and IManifestExpressionProvider but not IExpressionValue to be
    /// passed as parameters that can participate in reference expressions.
    /// This is required because the AWS aspire hosting library doesn't use the combined interface required downstream.
    /// </summary>
    class ExpressionValueAdapter : IExpressionValue
    {
        readonly object _backingValue;

        /// <summary>
        /// Allows classes that implement both IValueProvider and IManifestExpressionProvider 
        /// </summary>
        public ExpressionValueAdapter(object backingValue)
        {
            if (backingValue is not IValueProvider or not IManifestExpressionProvider)
            {
                throw new ArgumentException($"Backing value must implement both {nameof(IValueProvider)} and {nameof(IManifestExpressionProvider)}");
            }
            _backingValue = backingValue;
        }

        ValueTask<string?> IValueProvider.GetValueAsync(CancellationToken cancellationToken) => ((IValueProvider)_backingValue).GetValueAsync(cancellationToken);

        string IManifestExpressionProvider.ValueExpression => ((IManifestExpressionProvider)_backingValue).ValueExpression;
    }
}