namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests.Licensing;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

public class LicensingTests
{
    // Tests mutate process-wide PARTICULARSOFTWARE_LICENSE, PROGRAMDATA, and LOCALAPPDATA. Capture
    // and restore so a developer's real license/paths aren't clobbered by the test run.
    string? _originalLicenseEnv;
    string? _originalProgramData;
    string? _originalLocalAppData;
    List<string> _tempPaths = [];

    [SetUp]
    public void SetUp()
    {
        _originalLicenseEnv = Environment.GetEnvironmentVariable("PARTICULARSOFTWARE_LICENSE");
        _originalProgramData = Environment.GetEnvironmentVariable("PROGRAMDATA");
        _originalLocalAppData = Environment.GetEnvironmentVariable("LOCALAPPDATA");
        Environment.SetEnvironmentVariable("PARTICULARSOFTWARE_LICENSE", null);
        Environment.SetEnvironmentVariable("PROGRAMDATA", null);
        Environment.SetEnvironmentVariable("LOCALAPPDATA", null);
        _tempPaths = [];
    }

    [TearDown]
    public void TearDown()
    {
        Environment.SetEnvironmentVariable("PARTICULARSOFTWARE_LICENSE", _originalLicenseEnv);
        Environment.SetEnvironmentVariable("PROGRAMDATA", _originalProgramData);
        Environment.SetEnvironmentVariable("LOCALAPPDATA", _originalLocalAppData);
        foreach (var path in _tempPaths)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }

    [Test]
    public async Task LicenseShouldUseReadFromEnvVarByDefault()
    {
        var builder = new DistributedApplicationBuilder([]);
        Environment.SetEnvironmentVariable("PARTICULARSOFTWARE_LICENSE", Guid.NewGuid().ToString());

        builder.AddContainer("endpoint", "endpoint-container")
            .WithParticularPlatform(builder
                .AddParticularPlatform("particular")
                .WithTransportLearning()
            );

        var app = builder.Build();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();

        //assert that the endpoint resource has the license env var set from the provided file
        var license = model.Resources.Single(x => x.Name == "particular-license");
        Assert.That(license, Is.InstanceOf<ParameterResource>());
        var licenseParameter = (ParameterResource)license;

        var licenseValue = await licenseParameter.GetValueAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.That(licenseValue, Is.Not.Null.And.Not.Empty);
        Assert.That(licenseValue, Is.EqualTo(Environment.GetEnvironmentVariable("PARTICULARSOFTWARE_LICENSE")));
    }

    [Test]
    public async Task LicenseShouldOverriddenByText()
    {
        var builder = new DistributedApplicationBuilder([]);
        var text = Guid.NewGuid().ToString();

        builder.AddContainer("endpoint", "endpoint-container")
            .WithParticularPlatform(builder
                .AddParticularPlatform("particular")
                .WithLicenseFromText(text)
                .WithTransportLearning()
            );

        var app = builder.Build();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();

        //assert that the endpoint resource has the license text provided
        var license = model.Resources.Single(x => x.Name == "particular-license");
        Assert.That(license, Is.InstanceOf<ParameterResource>());
        var licenseParameter = (ParameterResource)license;

        var licenseValue = await licenseParameter.GetValueAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.That(licenseValue, Is.Not.Null.And.Not.Empty);
        Assert.That(licenseValue, Is.EqualTo(text));
    }

    [Test]
    public async Task LicenseShouldOverriddenByFile()
    {
        var builder = new DistributedApplicationBuilder([]);
        var text = Guid.NewGuid().ToString();
        var file = Path.GetTempFileName();
        await File.WriteAllTextAsync(file, text).ConfigureAwait(false);

        builder.AddContainer("endpoint", "endpoint-container")
            .WithParticularPlatform(builder
                .AddParticularPlatform("particular")
                .WithLicenseFromFile(file)
                .WithTransportLearning()
            );

        var app = builder.Build();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();

        //assert that the endpoint resource has the license text provided from the file
        var license = model.Resources.Single(x => x.Name == "particular-license");
        Assert.That(license, Is.InstanceOf<ParameterResource>());
        var licenseParameter = (ParameterResource)license;

        var licenseValue = await licenseParameter.GetValueAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.That(licenseValue, Is.Not.Null.And.Not.Empty);
        Assert.That(licenseValue, Is.EqualTo(text));
    }

    [Test]
    public async Task ParameterShouldOverrideLicense()
    {
        var builder = new DistributedApplicationBuilder([]);
        var text = Guid.NewGuid().ToString();
        var file = Path.GetTempFileName();
        await File.WriteAllTextAsync(file, text).ConfigureAwait(false);

        builder.AddContainer("endpoint", "endpoint-container")
            .WithParticularPlatform(builder
                .AddParticularPlatform("particular")
                .WithLicenseFromText("incorrect value")
                .WithTransportLearning()
            );

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Parameters:particular-license"] = text
        });

        var app = builder.Build();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();

        //assert that the endpoint resource has the license text provided from the parameter
        var license = model.Resources.Single(x => x.Name == "particular-license");
        Assert.That(license, Is.InstanceOf<ParameterResource>());
        var licenseParameter = (ParameterResource)license;

        var licenseValue = await licenseParameter.GetValueAsync(CancellationToken.None).ConfigureAwait(false);
        Assert.That(licenseValue, Is.Not.Null.And.Not.Empty);
        Assert.That(licenseValue, Is.EqualTo(text));
    }

    [Test]
    public async Task LicenseEnvVarShouldBeSetOnConsumerResource()
    {
        var builder = new DistributedApplicationBuilder([]);
        var text = Guid.NewGuid().ToString();

        builder.AddContainer("endpoint", "endpoint-container")
            .WithParticularPlatform(builder
                .AddParticularPlatform("particular")
                .WithLicenseFromText(text)
                .WithTransportLearning()
            );

        var app = builder.Build();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();

        var endpoint = model.Resources.OfType<IResourceWithEnvironment>().Single(x => x.Name == "endpoint");
        var executionContext = app.Services.GetRequiredService<DistributedApplicationExecutionContext>();
        var config = await ExecutionConfigurationBuilder
            .Create(endpoint)
            .WithEnvironmentVariablesConfig()
            .BuildAsync(executionContext)
            .ConfigureAwait(false);
        var envVars = config.EnvironmentVariables.ToDictionary(x => x.Key, x => x.Value);

        Assert.That(envVars, Contains.Key("PARTICULARSOFTWARE_LICENSE"));
        Assert.That(envVars["PARTICULARSOFTWARE_LICENSE"], Is.EqualTo(text));
    }

    [Test, CancelAfter(30_000)]
    public async Task LicenseShouldResolveFromProgramData(CancellationToken cancellationToken = default)
    {
        var text = Guid.NewGuid().ToString();
        Environment.SetEnvironmentVariable("PROGRAMDATA", CreateTempLicenseRoot(text));

        var licenseValue = await ResolveDefaultLicenseAsync(cancellationToken).ConfigureAwait(false);

        Assert.That(licenseValue, Is.EqualTo(text));
    }

    [Test, CancelAfter(30_000)]
    public async Task LicenseShouldResolveFromLocalAppDataWhenProgramDataAbsent(CancellationToken cancellationToken = default)
    {
        var text = Guid.NewGuid().ToString();
        Environment.SetEnvironmentVariable("LOCALAPPDATA", CreateTempLicenseRoot(text));

        var licenseValue = await ResolveDefaultLicenseAsync(cancellationToken).ConfigureAwait(false);

        Assert.That(licenseValue, Is.EqualTo(text));
    }

    [Test, CancelAfter(30_000)]
    public async Task ProgramDataShouldTakePrecedenceOverLocalAppDataAndEnvVar(CancellationToken cancellationToken = default)
    {
        var programDataText = Guid.NewGuid().ToString();
        var localAppDataText = Guid.NewGuid().ToString();
        var envVarText = Guid.NewGuid().ToString();

        Environment.SetEnvironmentVariable("PROGRAMDATA", CreateTempLicenseRoot(programDataText));
        Environment.SetEnvironmentVariable("LOCALAPPDATA", CreateTempLicenseRoot(localAppDataText));
        Environment.SetEnvironmentVariable("PARTICULARSOFTWARE_LICENSE", envVarText);

        var licenseValue = await ResolveDefaultLicenseAsync(cancellationToken).ConfigureAwait(false);

        Assert.That(licenseValue, Is.EqualTo(programDataText));
    }

    [Test, CancelAfter(30_000)]
    public async Task LicenseShouldFallBackToEmptyStringWhenNothingConfigured(CancellationToken cancellationToken = default)
    {
        var licenseValue = await ResolveDefaultLicenseAsync(cancellationToken).ConfigureAwait(false);

        Assert.That(licenseValue, Is.Empty);
    }

    [Test]
    public async Task WithLicenseFromTextShouldOverrideProgramDataFile()
    {
        Environment.SetEnvironmentVariable("PROGRAMDATA", CreateTempLicenseRoot("from-program-data"));
        var overrideText = Guid.NewGuid().ToString();

        var builder = new DistributedApplicationBuilder([]);
        builder.AddContainer("endpoint", "endpoint-container")
            .WithParticularPlatform(builder
                .AddParticularPlatform("particular")
                .WithLicenseFromText(overrideText)
                .WithTransportLearning()
            );

        var app = builder.Build();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var licenseParameter = (ParameterResource)model.Resources.Single(x => x.Name == "particular-license");

        var licenseValue = await licenseParameter.GetValueAsync(CancellationToken.None).ConfigureAwait(false);

        Assert.That(licenseValue, Is.EqualTo(overrideText));
    }

    [Test]
    public void WithLicenseFromTextShouldThrowOnNullOrEmpty()
    {
        var builder = new DistributedApplicationBuilder([]);
        var platform = builder.AddParticularPlatform("particular");

        Assert.Catch<ArgumentException>(() => platform.WithLicenseFromText(null!));
        Assert.Catch<ArgumentException>(() => platform.WithLicenseFromText(""));
    }

    [Test]
    public void WithLicenseFromFileShouldThrowOnNullOrEmpty()
    {
        var builder = new DistributedApplicationBuilder([]);
        var platform = builder.AddParticularPlatform("particular");

        Assert.Catch<ArgumentException>(() => platform.WithLicenseFromFile(null!));
        Assert.Catch<ArgumentException>(() => platform.WithLicenseFromFile(""));
    }

    [Test]
    public void WithLicenseFromFileShouldDeferValidationUntilResolution()
    {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xml");

        var builder = new DistributedApplicationBuilder([]);
        builder.AddContainer("endpoint", "endpoint-container")
            .WithParticularPlatform(builder
                .AddParticularPlatform("particular")
                .WithLicenseFromFile(nonExistentPath)
                .WithTransportLearning()
            );

        // Builder accepts a missing file. The contract is that validation is deferred to resolution.
        var app = builder.Build();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var licenseParameter = (ParameterResource)model.Resources.Single(x => x.Name == "particular-license");

        Assert.ThrowsAsync<FileNotFoundException>(async () =>
            await licenseParameter.GetValueAsync(CancellationToken.None).ConfigureAwait(false));
    }

    string CreateTempLicenseRoot(string licenseText)
    {
        var rootDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var particularDir = Path.Combine(rootDir, "ParticularSoftware");
        Directory.CreateDirectory(particularDir);
        File.WriteAllText(Path.Combine(particularDir, "license.xml"), licenseText);
        _tempPaths.Add(rootDir);
        return rootDir;
    }

    async Task<string?> ResolveDefaultLicenseAsync(CancellationToken cancellationToken)
    {
        var builder = new DistributedApplicationBuilder([]);
        builder.AddContainer("endpoint", "endpoint-container")
            .WithParticularPlatform(builder
                .AddParticularPlatform("particular")
                .WithTransportLearning()
            );

        var app = builder.Build();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();
        var licenseParameter = (ParameterResource)model.Resources.Single(x => x.Name == "particular-license");
        return await licenseParameter.GetValueAsync(cancellationToken).ConfigureAwait(false);
    }
}
