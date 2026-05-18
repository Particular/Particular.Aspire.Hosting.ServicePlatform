namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System;
using System.Linq;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public static class ParticularPlatformLicensingExtensions
{
    extension(IResourceBuilder<ParticularPlatformResource> platform)
    {
        /// <summary>
        /// Updates the default value for the platform license to be read from the provided file
        /// </summary>
        public IResourceBuilder<ParticularPlatformResource> WithLicenseFromFile(string file)
        {
            ArgumentException.ThrowIfNullOrEmpty(file);

            var licenseAnn = platform.Resource.Annotations.OfType<PlatformLicenseAnnotation>().Single();
            licenseAnn.License.Default = new ServicePlatformFileLicense(file);
            return platform;
        }

        /// <summary>
        /// Updates the default value for the platform license to be a specific string
        /// </summary>
        public IResourceBuilder<ParticularPlatformResource> WithLicenseFromText(string licenseText)
        {
            ArgumentException.ThrowIfNullOrEmpty(licenseText);

            var licenseAnn = platform.Resource.Annotations.OfType<PlatformLicenseAnnotation>().Single();
            licenseAnn.License.Default = new ServicePlatformTextLicense(licenseText);
            return platform;
        }
    }
}