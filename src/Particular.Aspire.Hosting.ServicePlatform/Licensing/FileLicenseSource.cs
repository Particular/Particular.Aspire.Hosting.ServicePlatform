namespace Particular.Aspire.Hosting.ServicePlatform.Licensing;

using System;
using System.Diagnostics.CodeAnalysis;

class FileLicenseSource(string file) : ILicenseSource
{
    public string File { get; } = file;

    public bool TryLoadText([NotNullWhen(true)] out string? text)
    {
        try
        {
            text = System.IO.File.Exists(File) ? System.IO.File.ReadAllText(File) : null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            text = null;
        }
        return text != null;
    }
}