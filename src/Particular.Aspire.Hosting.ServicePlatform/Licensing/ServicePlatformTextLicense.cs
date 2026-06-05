namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

sealed class ServicePlatformTextLicense(string licenseText) : LicenseParameterDefault(new TextLicenseSource(licenseText));