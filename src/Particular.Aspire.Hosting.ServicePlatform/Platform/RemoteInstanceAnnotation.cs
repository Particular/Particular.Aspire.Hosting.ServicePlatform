namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using global::Aspire.Hosting.ApplicationModel;

/// <summary>
/// defines a remote instance url for Service Control.
/// </summary>
/// <param name="endpoint"></param>
sealed class RemoteInstanceAnnotation(ReferenceExpression endpoint) : IResourceAnnotation
{
    public ReferenceExpression Endpoint { get; } = endpoint;
}