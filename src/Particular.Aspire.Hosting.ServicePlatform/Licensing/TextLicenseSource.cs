namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System.Diagnostics.CodeAnalysis;

class TextLicenseSource(string licenseText) : ILicenseSource
{
    public bool TryLoadText([NotNullWhen(true)] out string? text)
    {
        text = licenseText;
        return true;
    }
}