namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using Particular.Aspire.Hosting.ServicePlatform.Platform;
using global::Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;

public static class ParticularPlatformTransportExtensions
{
    extension(IResourceBuilder<ParticularPlatformResource> platform)
    {
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

        public IResourceBuilder<ParticularPlatformResource> WithTransportAzureServiceBus(
            IResourceBuilder<IResourceWithConnectionString> azureServiceBus)
        {
            ArgumentNullException.ThrowIfNull(azureServiceBus);
            return platform.WithAnnotation(new AzureServiceBusTransportAnnotation(azureServiceBus.Resource));
        }
    }
}