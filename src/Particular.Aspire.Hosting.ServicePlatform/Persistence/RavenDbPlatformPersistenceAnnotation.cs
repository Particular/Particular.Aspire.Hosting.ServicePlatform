namespace Particular.Aspire.Hosting.ServicePlatform.Persistence;

using global::Aspire.Hosting.ApplicationModel;

sealed class RavenDbPlatformPersistenceAnnotation(IResourceWithConnectionString connectionString) : IPlatformPersistenceAnnotation
{
    public IResource Resource { get; } = connectionString;

    void IPlatformPersistenceAnnotation.ApplyConfig(EnvironmentCallbackContext context)
    {
        context.EnvironmentVariables["RAVENDB_CONNECTIONSTRING"] = connectionString;
    }
}