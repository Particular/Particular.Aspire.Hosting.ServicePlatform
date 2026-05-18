namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System.IO;

class ServicePlatformFileLicense(string licensePath) : LicenseParameterDefault
{
    protected override string LoadLicenseText()
        => File.ReadAllText(licensePath);
}
