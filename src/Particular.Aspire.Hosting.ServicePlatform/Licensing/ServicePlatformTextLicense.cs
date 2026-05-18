namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

sealed class ServicePlatformTextLicense(string licenseText) : LicenseParameterDefault
{
    protected override string LoadLicenseText() => licenseText;
}
