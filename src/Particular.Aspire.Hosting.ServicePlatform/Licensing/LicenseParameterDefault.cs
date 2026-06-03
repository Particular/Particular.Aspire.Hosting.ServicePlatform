namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using global::Aspire.Hosting.ApplicationModel;
using global::Aspire.Hosting.Publishing;

class LicenseParameterDefault(params ILicenseSource[] searchLocations) : ParameterDefault
{
    string? _licenseText;

    public override void WriteToManifest(ManifestPublishingContext context)
        => context.Writer.WriteString("value", GetDefaultValue());

    public override string GetDefaultValue()
        => _licenseText ??= LoadLicenseText();

    string LoadLicenseText()
    {
        foreach (ILicenseSource searchLocation in searchLocations)
        {
            if (searchLocation.TryLoadText(out var text))
            {
                return text;
            }
        }
        return "";
    }
}