namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using global::Aspire.Hosting.ApplicationModel;
using global::Aspire.Hosting.Publishing;

abstract class LicenseParameterDefault : ParameterDefault
{
    string? _licenseText;

    public override void WriteToManifest(ManifestPublishingContext context)
        => context.Writer.WriteString("value", GetDefaultValue());

    public override string GetDefaultValue()
        => _licenseText ??= LoadLicenseText();

    protected abstract string LoadLicenseText();
}