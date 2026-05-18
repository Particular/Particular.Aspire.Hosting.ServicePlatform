namespace Particular.Aspire.Hosting.ServicePlatform.Persistence;

using global::Aspire.Hosting.ApplicationModel;

interface IPlatformPersistenceAnnotation : IResourceAnnotation
{
    IResource Resource { get; }
    void ApplyConfig(EnvironmentCallbackContext context);
}