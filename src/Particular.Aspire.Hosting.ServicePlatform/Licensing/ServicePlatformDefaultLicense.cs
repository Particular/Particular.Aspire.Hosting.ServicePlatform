namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System;

class ServicePlatformDefaultLicense() : LicenseParameterDefault(
    new FileLicenseSource(LicenseFileLocationResolver.ApplicationFolderLicenseFile),
    new FileLicenseSource(LicenseFileLocationResolver.GetPathFor(Environment.SpecialFolder.LocalApplicationData)),
    new FileLicenseSource(LicenseFileLocationResolver.GetPathFor(Environment.SpecialFolder.CommonApplicationData)),
    new EnvironmentVariableLicenseSource(PlatformEnvironment.ParticularSoftwareLicense),
    new TextLicenseSource("Trial")
);
