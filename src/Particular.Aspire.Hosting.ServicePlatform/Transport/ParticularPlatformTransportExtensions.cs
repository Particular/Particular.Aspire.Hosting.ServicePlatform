namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using Platform;
using global::Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using Aspire.ServicePlatform.Platform;

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


        public IResourceBuilder<ParticularPlatformResource> WithTransportAmazonSqs(IResourceBuilder<IResourceWithConnectionString> amazonSqs)
        {
            ArgumentNullException.ThrowIfNull(amazonSqs);

            return platform.WithAnnotation(new AmazonSqsTransportAnnotation(amazonSqs.Resource));
        }

    }
}
