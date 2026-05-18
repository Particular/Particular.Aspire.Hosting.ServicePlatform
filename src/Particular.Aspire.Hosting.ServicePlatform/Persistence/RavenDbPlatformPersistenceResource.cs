namespace Particular.Aspire.Hosting.ServicePlatform.Persistence;

using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public sealed class RavenDbPlatformPersistenceResource : ContainerResource, IResourceWithConnectionString
{
    internal RavenDbPlatformPersistenceResource([ResourceName] string name, ParticularPlatformResource parent)
        : base(name)
    {
        Parent = parent;
    }

    internal const string PrimaryEndpointName = "http";

    public ParticularPlatformResource Parent { get; }

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{Name}:{this.GetEndpoint(PrimaryEndpointName).Property(EndpointProperty.TargetPort)}");

}
