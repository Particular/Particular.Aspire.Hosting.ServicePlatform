//extension methods should be in the Aspire hosting namespace as per https://github.com/Particular/Particular.Aspire.Hosting.ServicePlatform/blob/main/docs/aspire-integration-guide.md#naming-conventions
namespace Aspire.Hosting;

using Particular.Aspire.Hosting.ServicePlatform.Platform;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
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
        /// <param name="storagePath">The file system path for message storage, relative paths are resolved relative to <see cref="DistributedApplicationBuilder.AppHostDirectory"/>. Defaults to using <c>.learningtransport</c> in the solution directory.</param>
        /// <returns>The platform resource builder for chaining.</returns>
        /// <exception cref="InvalidOperationException">Thrown if used in publish mode without enabling <c>Particular:AllowLearningTransportPublish</c>.</exception>
        public IResourceBuilder<ParticularPlatformResource> WithTransportLearning(string? storagePath = null)
        {
            if (storagePath == null)
            {
                storagePath = LearningTransportAnnotation.FindStoragePath();
            }
            else
            {
                storagePath = Path.IsPathRooted(storagePath)
                    ? storagePath
                    : Path.GetFullPath(storagePath, platform.ApplicationBuilder.AppHostDirectory);
            }

            Directory.CreateDirectory(storagePath);

            var transportConnection = platform.ApplicationBuilder
                .AddConnectionString($"learning-transport", ReferenceExpression.Create($"{storagePath}"))
                .WithParentRelationship(platform);

            platform.WithAnnotation(new LearningTransportAnnotation(storagePath, transportConnection.Resource));

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
        /// <param name="configure">Callback for configuring advanced options</param>
        /// <returns>The platform resource builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if required values are null.</exception>
        public IResourceBuilder<ParticularPlatformResource> WithTransportAmazonSqs(
            string region,
            IExpressionValue accessKeyId,
            IExpressionValue secretAccessKey,
            Action<AmazonSqsTransportSettings>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(region);
            ArgumentNullException.ThrowIfNull(accessKeyId);
            ArgumentNullException.ThrowIfNull(secretAccessKey);

            var options = new AmazonSqsTransportSettings
            {
                Region = region,
                AccessKeyId = accessKeyId,
                SecretAccessKey = secretAccessKey
            };

            configure?.Invoke(options);

            return platform.WithAnnotation(new AmazonSqsTransportAnnotation(options));
        }

        /// <summary>
        /// Configures the platform to use SQL Server as the message transport.
        /// </summary>
        /// <param name="sqlServer">The SQL Server resource providing the connection string.</param>
        /// <param name="queueSchema">An optional queue schema to append to the transport connection string.</param>
        /// <param name="subscriptionsTable">An optional subscriptions table to append to the transport connection string.</param>
        /// <returns>The platform resource builder for chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="sqlServer"/> is null.</exception>
        public IResourceBuilder<ParticularPlatformResource> WithTransportSqlServer(
            IResourceBuilder<IResourceWithConnectionString> sqlServer,
            string? queueSchema = null,
            string? subscriptionsTable = null)
        {
            ArgumentNullException.ThrowIfNull(sqlServer);
            return platform.WithAnnotation(new SqlServerTransportAnnotation(sqlServer.Resource, queueSchema, subscriptionsTable));
        }
    }
}