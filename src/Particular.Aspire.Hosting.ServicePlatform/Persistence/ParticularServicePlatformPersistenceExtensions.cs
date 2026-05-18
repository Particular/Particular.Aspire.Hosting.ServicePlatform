namespace Particular.Aspire.Hosting.ServicePlatform.Persistence;

using System;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;


public static class ParticularServicePlatformPersistenceExtensions
{
    extension(IResourceBuilder<ParticularPlatformResource> platform)
    {

        public IResourceBuilder<RavenDbPlatformPersistenceResource> AddPersistenceRavenDb(string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            var db = platform.ApplicationBuilder
                .AddResource(new RavenDbPlatformPersistenceResource(name, platform.Resource))
                .WithImage("particular/servicecontrol-ravendb", "latest")
                .WithHttpEndpoint(port: 8080, targetPort: 8080,
                    name: RavenDbPlatformPersistenceResource.PrimaryEndpointName)
                .WithUrlForEndpoint(RavenDbPlatformPersistenceResource.PrimaryEndpointName,
                    url => url.DisplayText = "Management Studio")
                .WithHttpHealthCheck("databases", endpointName: RavenDbPlatformPersistenceResource.PrimaryEndpointName);
            platform.WithPersistenceRavenDb(db);
            return db.WithParentRelationship(platform);
        }

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