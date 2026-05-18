namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using Particular.Aspire.Hosting.ServicePlatform.Platform;
using global::Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Linq;
using Aspire.ServicePlatform.Platform;
using global::Aspire.Hosting;

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

            var parameters = AmazonSqsTransportAnnotation.ParameterDefinitions
                .Where(p => !string.IsNullOrEmpty(platform.ApplicationBuilder.Configuration[p.ConfigurationSource]))
                .Select(p => (p.ConfigurationSource,
                    Value: platform.ApplicationBuilder.AddParameter(platform.Resource.Name + "-" + p.Name,
                        () => platform.ApplicationBuilder.Configuration[p.ConfigurationSource] ?? "",
                        secret: p.IsSecret).Resource))
                .ToDictionary(p => p.ConfigurationSource, p => p.Value);

            return platform.WithAnnotation(new AmazonSqsTransportAnnotation(amazonSqs.Resource, parameters));
        }

    }
}