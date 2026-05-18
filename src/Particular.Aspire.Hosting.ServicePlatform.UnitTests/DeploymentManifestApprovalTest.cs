namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Particular.Approvals;

[TestFixture]
public class DeploymentManifestApprovalTest
{
    static string AspireCliExe = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".aspire", "bin", OperatingSystem.IsWindows() ? "aspire.exe" : "aspire");

    [Test]
    public async Task TestDeploymentManifestAsync()
    {
        if (!File.Exists(AspireCliExe))
        {
            Assert.Ignore($"Aspire CLI not found at: {AspireCliExe}. Please install the Aspire CLI to run this test.");
        }

        var projectRoot = FindRootFolder();
        var outFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestDeploymentManifest");

        // aspire publish merges into the output folder rather than replacing it, so stale
        // entries (e.g. .env parameters from a previous AppHost configuration) can leak
        // into the next run and mask real changes. Delete the folder up-front for a clean run.
        if (Directory.Exists(outFolder))
        {
            Directory.Delete(outFolder, recursive: true);
        }

        var process = Process.Start(new ProcessStartInfo(AspireCliExe)
        {
            ArgumentList =
            {
                "publish",
                "--non-interactive",
                "-o", outFolder,
                "--apphost", Path.Combine(projectRoot, "AspireDemo/AspireDemo.AppHost/AspireDemo.AppHost.csproj")
            },
            UseShellExecute = true,
        })!;

        await process.WaitForExitAsync().ConfigureAwait(false);

        Assert.That(process.ExitCode, Is.EqualTo(0));
        Approver.Verify(string.Join("\r\n",
            Directory.GetFiles(outFolder, "*", SearchOption.AllDirectories)
                .OrderBy(x => x)
                .SelectMany(f => new[]
                {
                    "File: " + Path.GetRelativePath(outFolder, f), "==",
                    File.ReadAllText(f), "=="
                })));
    }

    /// <summary>
    /// Finds a solution folder upwards - or throws
    /// </summary>
    string FindRootFolder()
    {
        try
        {
            var currentDirectory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (currentDirectory != null)
            {
                Console.WriteLine(currentDirectory.FullName);
                if (currentDirectory.GetFiles("*.slnx", SearchOption.AllDirectories).Any())
                {
                    return currentDirectory.FullName;
                }

                currentDirectory = currentDirectory.Parent;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw new FileNotFoundException("Could not find a solution file", e);
        }

        throw new FileNotFoundException("Could not find a solution file");
    }
}