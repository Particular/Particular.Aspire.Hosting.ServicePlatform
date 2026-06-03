namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System;
using System.IO;
using System.Linq;
using Particular.Licensing;

class ServicePlatformDefaultLicense : LicenseParameterDefault
{
    protected override string LoadLicenseText()
    {
        string[] licenseLocations =
        [
            LicenseFileLocationResolver.ApplicationFolderLicenseFile,
            LicenseFileLocationResolver.GetPathFor(Environment.SpecialFolder.LocalApplicationData),
            LicenseFileLocationResolver.GetPathFor(Environment.SpecialFolder.CommonApplicationData)
        ];

        return licenseLocations
                   .Select(MaybeReadLicense)
                   .FirstOrDefault(x => x != null)
               ?? Environment.GetEnvironmentVariable(PlatformEnvironment.ParticularSoftwareLicense)
               ?? "";
    }

    static string? MaybeReadLicense(string? licensePath)
        => licensePath switch
        {
            null => null,
            _ when File.Exists(licensePath) => File.ReadAllText(licensePath),
            _ => null
        };
}
