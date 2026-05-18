namespace Particular.Aspire.ServicePlatform.Platform;

using System.Collections.Generic;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Hosting.ServicePlatform.Platform;
using Hosting.ServicePlatform.Transport;

sealed class AmazonSqsTransportAnnotation(
        IResourceWithConnectionString connectionSource,
        Dictionary<string, ParameterResource> parameters) : PlatformTransportAnnotation
{
    internal static readonly ParameterDetails[] ParameterDefinitions =
    [
        new("LICENSINGCOMPONENT-AMAZONSQS-ACCESSKEYID", "LICENSINGCOMPONENT_AMAZONSQS_ACCESSKEYID", false),
        new("LICENSINGCOMPONENT-AMAZONSQS-SECRETACCESSKEY", "LICENSINGCOMPONENT_AMAZONSQS_SECRETACCESSKEY", true),
        new("LICENSINGCOMPONENT-AMAZONSQS-REGION", "LICENSINGCOMPONENT_AMAZONSQS_REGION", false),
        new("LICENSINGCOMPONENT-AMAZONSQS-QUEUENAMEPREFIX", "LICENSINGCOMPONENT_AMAZONSQS_QUEUENAMEPREFIX", false),
        new("LICENSINGCOMPONENT-AMAZONSQS-TOPICNAMEPREFIX", "LICENSINGCOMPONENT_AMAZONSQS_TOPICNAMEPREFIX", false),
        new("LICENSINGCOMPONENT-AMAZONSQS-S3BUCKETFORLARGEMESSAGES", "LICENSINGCOMPONENT_AMAZONSQS_S3BUCKETFORLARGEMESSAGES", false),
        new("LICENSINGCOMPONENT-AMAZONSQS-S3KEYPREFIX", "LICENSINGCOMPONENT_AMAZONSQS_S3KEYPREFIX", false),
        new("LICENSINGCOMPONENT-AMAZONSQS-DONOTWRAPOUTGOINGMESSAGES", "LICENSINGCOMPONENT_AMAZONSQS_DONOTWRAPOUTGOINGMESSAGES", false),
        new("LICENSINGCOMPONENT-AMAZONSQS-RESERVEDBYTESINMESSAGESIZE", "LICENSINGCOMPONENT_AMAZONSQS_RESERVEDBYTESINMESSAGESIZE", false),
    ];

    public override string TransportType { get; } = "AmazonSQS";
    public override IResourceWithConnectionString ConnectionSource => connectionSource;

    public void ApplyTo<T>(IResourceBuilder<T> resource) where T : IResourceWithEnvironment
    {
        base.ApplyTo(resource);

        if (resource is IResourceBuilder<IPlatformComponent>)
        {
            if (resource is IResourceBuilder<ServiceControlErrorInstanceResource>)
            {
                foreach (var (key, parameter) in parameters)
                {
                    var paramBuilder = resource.ApplicationBuilder.CreateResourceBuilder(parameter);
                    resource.WithEnvironment(key, paramBuilder);
                }
            }
            return;
        }

        resource.WithReference(resource.ApplicationBuilder.CreateResourceBuilder(connectionSource));
    }

    internal sealed record ParameterDetails(
        string Name,
        string ConfigurationSource,
        bool IsSecret);
}