namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Licensing;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public static class ResourceBuilderExtensions
{
    extension<T>(IResourceBuilder<T> resource) where T : IResource
    {
        public IResourceBuilder<T> WithParticularPlatform(ParticularPlatformResource platform)
            => resource.WithParticularPlatform(resource.ApplicationBuilder.CreateResourceBuilder(platform));

        public IResourceBuilder<T> WithParticularPlatform(IResourceBuilder<ParticularPlatformResource> platform)
        {
            if (resource is IResourceBuilder<IResourceWithEnvironment> rwe)
            {
                rwe.WithLicense(platform);
                rwe.WithTransportFrom(platform);
            }

            return resource;
        }
    }

    extension<T>(IResourceBuilder<T> resource) where T : IResourceWithEnvironment
    {
        public IResourceBuilder<T> WithLicense(IResourceBuilder<ParticularPlatformResource> platform)
            => resource.WithEnvironment(PlatformLicenseAnnotation.LicenseEnvironmentVariable, platform.Resource.LicenseExpression);

        public IResourceBuilder<T> WithTransportFrom(IResourceBuilder<ParticularPlatformResource> platform)
        {
            platform.Resource.GetTransport().ApplyTo(resource);
            return resource;
        }
    }
}