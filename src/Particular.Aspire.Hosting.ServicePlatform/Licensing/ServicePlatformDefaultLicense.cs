namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System;

class ServicePlatformDefaultLicense() : LicenseParameterDefault(
    new FileLicenseSource(LicenseFileLocationResolver.ApplicationFolderLicenseFile),
    new FileLicenseSource(LicenseFileLocationResolver.GetPathFor(Environment.SpecialFolder.LocalApplicationData)),
    new FileLicenseSource(LicenseFileLocationResolver.GetPathFor(Environment.SpecialFolder.CommonApplicationData)),
    new EnvironmentVariableLicenseSource(PlatformEnvironment.ParticularSoftwareLicense),
    new TextLicenseSource(TrialLicensePlaceholder)
)
{
    /// <summary>
    /// Placeholder value used to allow the license parameter to be valid and allow the Aspire resources to start up.
    /// This value will fail to validate in ServiceControl and any endpoints triggering the trial license to be loaded.
    /// </summary>
    public const string TrialLicensePlaceholder = "Trial";
}

