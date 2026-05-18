namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using Particular.Aspire.Hosting.ServicePlatform.Platform;
using global::Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;

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
            var resolvedPath = Path.GetFullPath(storagePath ?? ".learningtransport");
            Directory.CreateDirectory(resolvedPath);
            platform.WithAnnotation(new LearningTransportAnnotation(resolvedPath));

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
    }
}