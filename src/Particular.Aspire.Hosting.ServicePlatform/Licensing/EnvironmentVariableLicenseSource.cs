namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System;
using System.Diagnostics.CodeAnalysis;

class EnvironmentVariableLicenseSource(string variable) : ILicenseSource
{
    public bool TryLoadText([NotNullWhen(true)] out string? text)
    {
        text = Environment.GetEnvironmentVariable(variable);
        return !string.IsNullOrEmpty(text);
    }
}