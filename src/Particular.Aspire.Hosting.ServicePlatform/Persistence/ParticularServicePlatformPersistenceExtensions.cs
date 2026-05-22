//extension methods should be in the Aspire hosting namespace as per https://github.com/Particular/Particular.Aspire.Hosting.ServicePlatform/blob/main/docs/aspire-integration-guide.md#naming-conventions
namespace Aspire.Hosting;

using System;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Persistence;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Extension methods for configuring persistence instances for the Particular Service Platform within Aspire
/// </summary>
public static class ParticularServicePlatformPersistenceExtensions
{
    extension(IResourceBuilder<ParticularPlatformResource> platform)
    {

        /// <summary>
        /// Adds an internally managed RavenDB persistence instance to the platform resource.
        /// </summary>
        /// <param name="name">The aspire name for the resource</param>
        /// <returns>The configured RavenDB resource</returns>
        /// <exception cref="ArgumentException">Thrown if name is null or empty</exception>
        public IResourceBuilder<RavenDbPlatformPersistenceResource> AddPersistenceRavenDb(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            var db = platform.ApplicationBuilder
                .AddResource(new RavenDbPlatformPersistenceResource(name, platform.Resource))
                .WithImage("particular/servicecontrol-ravendb", "latest")
                .WithHttpEndpoint(targetPort: 8080, name: RavenDbPlatformPersistenceResource.PrimaryEndpointName)
                .WithUrlForEndpoint(RavenDbPlatformPersistenceResource.PrimaryEndpointName,
                    url => url.DisplayText = "Management Studio")
                .WithHttpHealthCheck("databases", endpointName: RavenDbPlatformPersistenceResource.PrimaryEndpointName);
            platform.WithPersistenceRavenDb(db);
            return db.WithParentRelationship(platform);
        }

        /// <summary>
        /// Attach an existing RavenDB persistence instance to the platform resource.
        /// </summary>
        /// <param name="persistence">The RavenDB persistence instance to attach</param>
        /// <returns>The configured platform resource</returns>
        /// <exception cref="ArgumentNullException">Thrown if persistence is null</exception>
        public IResourceBuilder<ParticularPlatformResource> WithPersistenceRavenDb<TConnection>(
            IResourceBuilder<TConnection> persistence)
            where TConnection : IResourceWithConnectionString
        {
            ArgumentNullException.ThrowIfNull(persistence);

            return platform
                .WithAnnotation(new RavenDbPlatformPersistenceAnnotation(persistence.Resource))
                .WithRelationship(persistence.Resource, "Particular.Persistence");
        }
    }
}