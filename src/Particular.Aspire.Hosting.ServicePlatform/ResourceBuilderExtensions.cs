namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Licensing;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Extension methods for connecting any Aspire resource to the Particular Service Platform,
/// providing license and transport configuration.
/// </summary>
public static class ResourceBuilderExtensions
{
    extension<T>(IResourceBuilder<T> resource) where T : IResource
    {
        /// <summary>
        /// Connects this resource to the Particular Service Platform, applying the platform license
        /// and transport configuration to the resource's environment.
        /// </summary>
        /// <param name="platform">The platform resource to connect to.</param>
        /// <returns>The resource builder for chaining.</returns>
        public IResourceBuilder<T> WithParticularPlatform(ParticularPlatformResource platform)
            => resource.WithParticularPlatform(resource.ApplicationBuilder.CreateResourceBuilder(platform));

        /// <summary>
        /// Connects this resource to the Particular Service Platform, applying the platform license
        /// and transport configuration to the resource's environment.
        /// </summary>
        /// <param name="platform">The platform resource builder to connect to.</param>
        /// <returns>The resource builder for chaining.</returns>
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
        /// <summary>
        /// Injects the Particular Service Platform license into this resource's environment variables.
        /// </summary>
        /// <param name="platform">The platform resource builder containing the license configuration.</param>
        /// <returns>The resource builder for chaining.</returns>
        public IResourceBuilder<T> WithLicense(IResourceBuilder<ParticularPlatformResource> platform)
            => resource.WithEnvironment(PlatformLicenseAnnotation.LicenseEnvironmentVariable, platform.Resource.LicenseExpression);

        /// <summary>
        /// Applies the platform's transport configuration to this resource, setting the appropriate
        /// connection string or transport-specific environment variables.
        /// </summary>
        /// <param name="platform">The platform resource builder containing the transport configuration.</param>
        /// <returns>The resource builder for chaining.</returns>
        public IResourceBuilder<T> WithTransportFrom(IResourceBuilder<ParticularPlatformResource> platform)
        {
            platform.Resource.GetTransport().ApplyTo(resource);
            return resource;
        }
    }
}