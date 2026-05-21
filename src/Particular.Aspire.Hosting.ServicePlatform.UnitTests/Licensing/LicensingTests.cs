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
    [SetUp]
    public void OneTimeSetUp() =>
        // Clear the environment variable to ensure tests are not affected by a global environment
        Environment.SetEnvironmentVariable("PARTICULARSOFTWARE_LICENSE", null);

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
}
