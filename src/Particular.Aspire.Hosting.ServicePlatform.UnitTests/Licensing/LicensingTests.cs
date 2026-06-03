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
using ServicePlatform.Licensing;

public class LicensingTests
{
    List<string> _tempPaths = [];

    [SetUp]
    public void SetUp() => _tempPaths = [];

    [TearDown]
    public void TearDown()
    {
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

    [TestCase(null, true)]
    [TestCase("", true)]
    [TestCase("a string", true)]
    public void TestTextLicenseSource(string? text, bool expected)
    {
        var source = new TextLicenseSource(text!);
        Assert.That(source.TryLoadText(out var loadedText), Is.EqualTo(expected));
        Assert.That(loadedText, Is.EqualTo(text));
    }

    [Test]
    public void TestFileLicenseSourceReturnsLicenseFromFile()
    {
        var text = Guid.NewGuid().ToString();
        var file = Path.Combine(CreateTempLicenseRoot(text), "ParticularSoftware/license.xml");
        var source = new FileLicenseSource(file);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(source.TryLoadText(out var result), Is.True);
            Assert.That(result, Is.EqualTo(text));
        }
    }

    [Test]
    public void TestFileLicenseSourceReturnsFalseForNoFile()
    {
        var source = new FileLicenseSource($"/this/file/should/not/exist/{Guid.NewGuid():N}.xml");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(source.TryLoadText(out var result), Is.False);
            Assert.That(result, Is.Null);
        }
    }

    [TestCase(null, false)]
    [TestCase("", false)]
    [TestCase("value", true)]
    public void TestEnvironmentVariableLicenseSourceReturnsValueEnvVar(string? value, bool expected)
    {
        var varName = Guid.NewGuid().ToString("N");
        Environment.SetEnvironmentVariable(varName, value);
        var source = new EnvironmentVariableLicenseSource(varName);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(source.TryLoadText(out var result), Is.EqualTo(expected));
            if (expected)
            {
                Assert.That(result, Is.EqualTo(value));
            }
        }
    }

    [Test]
    public void AddParticularPlatformAddsLicenseDefault()
    {
        var builder = new DistributedApplicationBuilder([]);

        builder.AddContainer("endpoint", "endpoint-container")
            .WithParticularPlatform(builder
                .AddParticularPlatform("particular")
                .WithTransportLearning()
            );

        var app = builder.Build();
        var model = app.Services.GetRequiredService<DistributedApplicationModel>();

        var gotLicense = model.Resources.TryGetByName("particular-license", out var licenseResource);
        Assert.That(gotLicense, Is.True);
        Assert.That(licenseResource, Is.InstanceOf<ParameterResource>());
        Assert.That(((ParameterResource)licenseResource).Default, Is.InstanceOf<ServicePlatformDefaultLicense>());
    }

    [Test]
    public async Task LicenseShouldBeOverriddenByText()
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
        Assert.That(licenseValue, Is.EqualTo(text));
    }

    [Test]
    public async Task LicenseShouldBeOverriddenByFile()
    {
        var builder = new DistributedApplicationBuilder([]);
        var text = Guid.NewGuid().ToString();
        var file = Path.Combine(CreateTempLicenseRoot(text), "ParticularSoftware/license.xml");

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
        Assert.That(licenseValue, Is.EqualTo(text));
    }

    [Test]
    public async Task ParameterShouldOverrideLicense()
    {
        var builder = new DistributedApplicationBuilder([]);
        var text = Guid.NewGuid().ToString();

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
    public void LicenseParameterDefaultShouldReturnEmptyIfNoSourcesOrdering()
    {
        Assert.That(new LicenseParameterDefault().GetDefaultValue(), Is.Empty);
        Assert.That(new LicenseParameterDefault(new FileLicenseSource(Guid.NewGuid().ToString("N"))).GetDefaultValue(), Is.Empty);
        Assert.That(new LicenseParameterDefault([
            new FileLicenseSource(Guid.NewGuid().ToString("N")),
            new FileLicenseSource(Guid.NewGuid().ToString("N")),
            new FileLicenseSource(Guid.NewGuid().ToString("N"))
        ]).GetDefaultValue(), Is.Empty);
    }

    [Test]
    public void LicenseParameterShouldReturnFirstNonEmptyLicenseFromParamsOrdering()
    {
        var text = Guid.NewGuid().ToString();
        var text2 = Guid.NewGuid().ToString();
        var file = Path.Combine(CreateTempLicenseRoot(text), "ParticularSoftware/license.xml");
        var file2 = Path.Combine(CreateTempLicenseRoot(text2), "ParticularSoftware/license.xml");

        var result = new LicenseParameterDefault([
            new FileLicenseSource(Guid.NewGuid().ToString("N")),
            new FileLicenseSource(file),
            new FileLicenseSource(file2)
        ]).GetDefaultValue();
        Assert.That(result, Is.EqualTo(text));
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
