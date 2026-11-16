namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests.TestResources;

using System;
using System.Collections.Generic;
using System.IO;
using global::Aspire.Hosting;
using Microsoft.Extensions.Configuration;

public class TestPublishingContext : IDisposable
{
    public TestPublishingContext()
    {
        OutDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", ""));
        AspireHome = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", ""));

        Builder = new DistributedApplicationBuilder(new DistributedApplicationOptions
        {
            Args = ["--operation", "publish", "--step", "publish", "--output-path", OutDir],
            DisableDashboard = true
        });

        Builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Particular:AllowLearningTransportPublish"] = "true",
            // Aspire persists parameter values (like the license) under ASPIRE_HOME between publish runs, so each test gets its own
            ["ASPIRE_HOME"] = AspireHome,
            ["ASPIRE_DCP_PATH"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aspire/bin/dcp"),
            ["ASPIRE_DASHBOARD_PATH"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aspire/managed/wwwroot")
        });
    }

    public string OutDir { get; set; }

    string AspireHome { get; }

    public DistributedApplicationBuilder Builder { get; set; }

    public void Dispose()
    {
        if (File.Exists(OutDir))
        {
            Directory.Delete(OutDir, true);
        }
        if (Directory.Exists(OutDir))
        {
            Directory.Delete(OutDir, true);
        }
        if (Directory.Exists(AspireHome))
        {
            Directory.Delete(AspireHome, true);
        }
    }
}