namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using System;
using System.IO;
using System.Linq;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Configures the Learning transport for the Particular Service Platform. The Learning transport
/// stores messages on the local file system and is intended for development and testing scenarios only.
/// </summary>
sealed class LearningTransportAnnotation(string storagePath, IResourceWithConnectionString connectionString) : IPlatformTransportAnnotation
{
    const string ContainerPath = "/tmp/learningtransport";
    const string DefaultLearningTransportDirectory = ".learningtransport";

    /// <summary>
    /// The configuration key that must be set to <c>true</c> to allow the Learning transport in publish mode.
    /// </summary>
    public const string SettingsEnablePublish = "Particular:AllowLearningTransportPublish";

    /// <inheritdoc />
    public void ApplyTo<T>(IResourceBuilder<T> resource) where T : IResourceWithEnvironment
    {
        if (resource is IResourceBuilder<IPlatformComponent>)
        {
            resource.WithEnvironment(context =>
            {
                context.EnvironmentVariables[PlatformEnvironment.ServiceControl.TransportType] = "LearningTransport";
                context.EnvironmentVariables[PlatformEnvironment.ServiceControl.ConnectionString] = ContainerPath;
            });

            if (resource is not IResourceBuilder<ContainerResource> container)
            {
                throw new InvalidOperationException(
                    "The LearningTransport is only supported when the platform is hosted in a container.");
            }

            container.WithBindMount(storagePath!, ContainerPath, isReadOnly: false);
            return;
        }

        resource.WithReference(resource.ApplicationBuilder.CreateResourceBuilder(connectionString));
    }

    /// <summary>
    /// Finds the directory of the learning transport for this project.
    /// </summary>
    /// <remarks>
    /// The behavior of this path search should mirror the implementation in the learning transport (https://github.com/Particular/NServiceBus/blob/master/src/NServiceBus.Core/Transports/Learning/LearningTransportInfrastructure.cs)
    /// </remarks>
    internal static string FindStoragePath()
    {
        var directory = AppDomain.CurrentDomain.BaseDirectory;

        while (true)
        {
            // Finding a solution file takes precedence
            if (Directory.EnumerateFiles(directory).Any(file => Path.GetExtension(file) is ".sln" or ".slnx"))
            {
                return Path.Combine(directory, DefaultLearningTransportDirectory);
            }

            // When no solution file was found try to find a learning transport directory
            var learningTransportDirectory = Path.Combine(directory, DefaultLearningTransportDirectory);
            if (Directory.Exists(learningTransportDirectory))
            {
                return learningTransportDirectory;
            }

            var parent = Directory.GetParent(directory) ?? throw new Exception($"Unable to determine the storage directory path for the learning transport due to the absence of a solution file. Either create a '{DefaultLearningTransportDirectory}' directory in one of this project’s parent directories, or specify the path explicitly when adding the transport.");

            directory = parent.FullName;
        }
    }

}