namespace Particular.Aspire.Hosting.ServicePlatform.Persistence;

using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Represents the RavenDB persistence resource for the Particular Service Platform. This resource uses the pre-configured
/// RavenDB container image to provide a ready-to-use RavenDB instance for the service platform.
/// </summary>
public sealed class RavenDbPlatformPersistenceResource : ContainerResource, IResourceWithConnectionString
{
    internal RavenDbPlatformPersistenceResource([ResourceName] string name, ParticularPlatformResource parent)
        : base(name)
    {
        Parent = parent;
    }

    internal const string PrimaryEndpointName = "http";

    /// <summary>
    /// The parent platform that this instance belongs to
    /// </summary>
    public ParticularPlatformResource Parent { get; }

    /// <summary>
    /// The connection string to the database
    /// </summary>
    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create(
            $"http://{Name}:{this.GetEndpoint(PrimaryEndpointName).Property(EndpointProperty.TargetPort)}");

}
