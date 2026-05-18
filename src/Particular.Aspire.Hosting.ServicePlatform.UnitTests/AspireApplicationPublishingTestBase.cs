namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Particular.Approvals;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

/// <summary>
/// Base class for a test that performs an approval test on the publish output on an Aspire distributed application.
/// </summary>
public abstract class AspireApplicationPublishingTestBase
{
    protected abstract void BuildApplication(IDistributedApplicationBuilder builder);

    [Test, CancelAfter(30_000)]
#pragma warning disable PS0003 // A parameter of type CancellationToken on a non-private delegate or method should be optional
    public async Task ApprovePublishOutput(CancellationToken cancellationToken)
#pragma warning restore PS0003 // A parameter of type CancellationToken on a non-private delegate or method should be optional
    {
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", ""));
        try
        {
            await TestContext.Out.WriteLineAsync($"Creating application").ConfigureAwait(false);
            var builder = new DistributedApplicationBuilder(new DistributedApplicationOptions()
            {
                Args = [
                    "--operation", "publish",
                    "--step", "publish",
                    "--output-path", outDir,
                ],
                DisableDashboard = true
            });

            // Publish does not use DCP - but it does validate this config object, so give it some sane values.
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                [LearningTransportAnnotation.SettingsEnablePublish] = "true",
                ["ASPIRE_DCP_PATH"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aspire/bin/dcp"),
                ["ASPIRE_DASHBOARD_PATH"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aspire/managed/wwwroot")
            });

            BuildApplication(builder);

            if (!builder.Resources.OfType<IComputeEnvironmentResource>().Any())
            {
                throw new InvalidOperationException("The application must contain at least one compute environment resource (e.g. a Docker environment) to be publishable.");
            }

            var app = builder.Build();

            await TestContext.Out.WriteLineAsync($"Running application").ConfigureAwait(false);
            await app.RunAsync(cancellationToken).ConfigureAwait(false);

            await TestContext.Out.WriteLineAsync($"Verifying output directory: {outDir}").ConfigureAwait(false);

            Approver.Verify(string.Join("\r\n",
                Directory.GetFiles(outDir, "*", SearchOption.AllDirectories)
                    .OrderBy(x => x)
                    .SelectMany(f => new[]
                    {
                        // need to normalise path separators so approval succeeds on both Windows and Unix
                        "File: " + Path.GetRelativePath(outDir, f).Replace("\\", "/"), "==",
                        File.ReadAllText(f), "=="
                    })),
                // ReSharper disable once ExplicitCallerInfoArgument
                // explicit override here to make the abstract base class look right in the approvals folder
                callerFilePath: GetType().Name
            );
        }
        finally
        {
            if (File.Exists(outDir))
            {
                Directory.Delete(outDir, true);
            }
            if (Directory.Exists(outDir))
            {
                Directory.Delete(outDir, true);
            }
        }
    }
}