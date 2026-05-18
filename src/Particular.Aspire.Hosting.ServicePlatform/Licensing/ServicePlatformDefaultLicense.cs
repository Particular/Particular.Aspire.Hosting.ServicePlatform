namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System;
using System.IO;

class ServicePlatformDefaultLicense : LicenseParameterDefault
{
    protected override string LoadLicenseText()
        => MaybeReadLicense(Environment.GetEnvironmentVariable("PROGRAMDATA"))
        ?? MaybeReadLicense(Environment.GetEnvironmentVariable("LOCALAPPDATA"))
        ?? Environment.GetEnvironmentVariable(PlatformLicenseAnnotation.LicenseEnvironmentVariable)
        ?? "";

    static string? MaybeReadLicense(string? rootPath)
        => rootPath switch
        {
            null => null,
            _ => Path.Combine(rootPath, "ParticularSoftware", "license.xml") switch
            {
                var licensePath => File.Exists(licensePath)
                    ? File.ReadAllText(licensePath)
                    : null
            }
        };
}
