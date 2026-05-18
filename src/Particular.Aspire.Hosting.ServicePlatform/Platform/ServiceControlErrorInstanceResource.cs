namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using System;
using System.Linq;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Persistence;

/// <summary>
/// Represents a ServiceControl Error instance running as a container resource within the Particular Service Platform.
/// The error instance ingests failed messages, manages retries, and serves as the primary ServiceControl API endpoint.
/// </summary>
public sealed class ServiceControlErrorInstanceResource : ContainerResource, IPlatformComponent,
    IResourceWithParent<ParticularPlatformResource>
{
    internal ServiceControlErrorInstanceResource([ResourceName] string name, ParticularPlatformResource parent) :
        base(name)
    {
        Parent = parent;
        Annotations.Add(new EnvironmentCallbackAnnotation(context =>
        {
            if (!this.TryGetLastAnnotation<IPlatformPersistenceAnnotation>(out var annotation))
            {
                throw new InvalidOperationException($"No persistence found for {Name}");
            }

            annotation.ApplyConfig(context);
            context.EnvironmentVariables["REMOTEINSTANCES"] = RemoteInstancesExpression;
        }));
    }

    internal const string ErrorEndpointName = "error";
    internal const string ThroughputQueueEnvVar = "LICENSINGCOMPONENT_SERVICECONTROLTHROUGHPUTDATAQUEUE";
    internal const string ErrorQueueEnvVar = "SERVICEBUS_ERRORQUEUE";
    internal const string DefaultErrorQueueName = "error";
    /// <summary>
    /// The parent platform resource that this error instance belongs to.
    /// </summary>
    public ParticularPlatformResource Parent { get; }

    ReferenceExpression RemoteInstancesExpression
    {
        get
        {
            var builder = new ReferenceExpressionBuilder();

            //cannot use braces in the builder.Append because it unescapes them at the next layer
            //so precompute them
            var jsonNodes = Annotations
                .OfType<RemoteInstanceAnnotation>()
                .Select(x =>
                    ReferenceExpression.Create($"{{\"api_uri\": \"{x.Endpoint}\"}}")
                ).ToList();

            builder.AppendLiteral("[");
            for (var index = 0; index < jsonNodes.Count; index++)
            {
                var node = jsonNodes[index];
                if (index > 0)
                {
                    builder.AppendLiteral(",");
                }
                builder.Append($"{node}");
            }

            builder.AppendLiteral("]");
            return builder.Build();
        }
    }
}
