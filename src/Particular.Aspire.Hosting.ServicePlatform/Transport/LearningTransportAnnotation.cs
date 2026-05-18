namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using System;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public sealed class LearningTransportAnnotation(string storagePath) : IPlatformTransportAnnotation
{
    internal const string ContainerPath = "/tmp/learningtransport";
    public const string SettingsEnablePublish = "Particular:AllowLearningTransportPublish";

    public void ApplyTo<T>(IResourceBuilder<T> resource) where T : IResourceWithEnvironment
    {
        if (resource is IResourceBuilder<IPlatformComponent>)
        {
            resource.WithEnvironment(context =>
            {
                context.EnvironmentVariables["TRANSPORTTYPE"] = "LearningTransport";
                context.EnvironmentVariables["CONNECTIONSTRING"] = ContainerPath;
            });

            if (resource is not IResourceBuilder<ContainerResource> container)
            {
                throw new InvalidOperationException(
                    "The LearningTransport is only supported when the platform is hosted in a container.");
            }

            container.WithBindMount(storagePath!, ContainerPath, isReadOnly: false);
            return;
        }

        resource.WithEnvironment("LEARNING_TRANSPORT_PATH", storagePath!);
    }
}