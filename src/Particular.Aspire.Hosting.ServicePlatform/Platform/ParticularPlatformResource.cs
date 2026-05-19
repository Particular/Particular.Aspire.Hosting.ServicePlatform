namespace Particular.Aspire.Hosting.ServicePlatform.Platform;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Licensing;
using Particular.Aspire.Hosting.ServicePlatform.Persistence;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

/// <summary>
/// The parent resource that represents the grouping of the Particular Service Platform.
/// </summary>
/// <remarks>
/// Synthetic grouping resource — no process of its own. All topology config is attached via
/// annotations, and children are discovered by walking <see cref="IResourceWithParent{T}"/> where T is <see cref="ParticularPlatformResource"/>.
/// AddParticularPlatform sets its initial state to Starting and calls ExcludeFromManifest().
/// </remarks>
public sealed class ParticularPlatformResource : Resource
{
    internal ParticularPlatformResource([ResourceName] string name) : base(name)
    {
    }

    /// <summary>
    /// This expression will provide the license information to be provided to endpoints and platform components.
    /// </summary>
    public ReferenceExpression LicenseExpression => this.TryGetLastAnnotation<PlatformLicenseAnnotation>(out var la)
        ? ReferenceExpression.Create($"{la.License}")
        : ReferenceExpression.Create($"");

    internal IPlatformTransportAnnotation GetTransport() => this.TryGetLastAnnotation<IPlatformTransportAnnotation>(out var ta)
        ? ta
        : throw new InvalidOperationException($"No transport configured for platform {Name}.");

    internal bool TryGetPersistenceConfig(IResource persistenceResource, [NotNullWhen(true)] out IPlatformPersistenceAnnotation? persistenceAnnotation)
    {
        persistenceAnnotation = Annotations
            .OfType<IPlatformPersistenceAnnotation>()
            .LastOrDefault(x => x.Resource == persistenceResource);

        return persistenceAnnotation != null;
    }
}
