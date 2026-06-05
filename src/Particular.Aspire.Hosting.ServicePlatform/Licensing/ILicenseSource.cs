namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System.Diagnostics.CodeAnalysis;

interface ILicenseSource
{
    bool TryLoadText(
        [NotNullWhen(true)]
        out string? text);
}