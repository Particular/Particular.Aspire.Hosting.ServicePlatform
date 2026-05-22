namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using global::Aspire.Hosting.ApplicationModel;

// ParameterResource isn't an IResourceWithParent, so the children walk can't discover it.
// Holding it on an annotation lets WithLicenseFromFile/WithLicenseText mutate its Default
// after AddParticularPlatform has already returned.
sealed class PlatformLicenseAnnotation(ParameterResource license) : IResourceAnnotation
{
    public const string LicenseEnvironmentVariable = "PARTICULARSOFTWARE_LICENSE";

    public ParameterResource License { get; } = license;
}
