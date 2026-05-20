//extension methods should be in the Aspire hosting namespace as per https://github.com/Particular/Particular.Aspire.Hosting.ServicePlatform/blob/main/docs/aspire-integration-guide.md#naming-conventions
namespace Aspire.Hosting;

using System;
using System.Linq;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Licensing;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Extension methods for configuring licensing for the Particular Service Platform within Aspire
/// </summary>
public static class ParticularPlatformLicensingExtensions
{
    extension(IResourceBuilder<ParticularPlatformResource> platform)
    {
        /// <summary>
        /// Updates the default value for the platform license to be read from the provided file.
        /// </summary>
        /// <param name="file">Path to the license file.</param>
        /// <returns>The platform resource builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="file"/> is null or empty.</exception>
        public IResourceBuilder<ParticularPlatformResource> WithLicenseFromFile(string file)
        {
            ArgumentException.ThrowIfNullOrEmpty(file);

            var licenseAnn = platform.Resource.Annotations.OfType<PlatformLicenseAnnotation>().Single();
            licenseAnn.License.Default = new ServicePlatformFileLicense(file);
            return platform;
        }

        /// <summary>
        /// Updates the default value for the platform license to be a specific string.
        /// </summary>
        /// <param name="licenseText">The license XML content.</param>
        /// <returns>The platform resource builder for chaining.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="licenseText"/> is null or empty.</exception>
        public IResourceBuilder<ParticularPlatformResource> WithLicenseFromText(string licenseText)
        {
            ArgumentException.ThrowIfNullOrEmpty(licenseText);

            var licenseAnn = platform.Resource.Annotations.OfType<PlatformLicenseAnnotation>().Single();
            licenseAnn.License.Default = new ServicePlatformTextLicense(licenseText);
            return platform;
        }
    }
}