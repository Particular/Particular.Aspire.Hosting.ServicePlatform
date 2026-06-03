namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

class ServicePlatformFileLicense(string licensePath) : LicenseParameterDefault(new FileLicenseSource(licensePath));