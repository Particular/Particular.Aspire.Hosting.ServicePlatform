namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System.Diagnostics.CodeAnalysis;
using System.IO;

class FileLicenseSource(string file) : ILicenseSource
{
    public bool TryLoadText([NotNullWhen(true)] out string? text)
    {
        text = File.Exists(file) ? File.ReadAllText(file) : null;
        return text != null;
    }
}