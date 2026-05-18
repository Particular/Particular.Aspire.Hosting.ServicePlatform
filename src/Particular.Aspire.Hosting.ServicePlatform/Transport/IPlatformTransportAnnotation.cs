namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting.ApplicationModel;

interface IPlatformTransportAnnotation : IResourceAnnotation
{
    void ApplyTo<T>(IResourceBuilder<T> resource) where T : IResourceWithEnvironment;
}