namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using global::Aspire.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NUnit.Framework;
using Particular.Aspire.Hosting.ServicePlatform.Tests.TestResources;

public class MismatchedServiceControlVersionsTest
{
    [Test, CancelAfter(30_000)]
    public async Task Should_warn_when_servicecontrol_image_versions_differ(CancellationToken cancellationToken = default)
    {
        var collector = new FakeLogCollector();

        var builder = CreatePublishBuilder();

        builder.Services.AddLogging(logging => logging.AddProvider(new FakeLoggerProvider(collector)));

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAzureServiceBus(builder.AddDummyConnectionString("transport-connection"));

        var persistence = platform.AddPersistenceRavenDb("particular-persistence");

        platform.AddServiceControlErrorInstance("particular-error", persistence)
            .WithImage("particular/servicecontrol", "6.0.0");

        platform.AddServiceControlMonitoringInstance("particular-monitoring")
            .WithImage("particular/servicecontrol-monitoring", "5.0.0");

        builder.AddDockerComposeEnvironment("compose");

        var app = builder.Build();
        await app.RunAsync(cancellationToken).ConfigureAwait(false);

        var warnings = collector.GetSnapshot()
            .Where(r => r.Level == LogLevel.Warning)
            .ToList();

        Assert.That(warnings, Has.Exactly(1).Matches<FakeLogRecord>(r =>
            r.Message.Contains("mismatched ServiceControl container image versions")));
    }

    [Test, CancelAfter(30_000)]
    public async Task Should_not_warn_when_servicecontrol_image_versions_are_aligned(CancellationToken cancellationToken = default)
    {
        var collector = new FakeLogCollector();

        var builder = CreatePublishBuilder();

        builder.Services.AddLogging(logging => logging.AddProvider(new FakeLoggerProvider(collector)));

        builder
            .AddParticularPlatform("particular")
            .AddDefaultComponents();

        builder.AddDockerComposeEnvironment("compose");

        var app = builder.Build();
        await app.RunAsync(cancellationToken).ConfigureAwait(false);

        var warnings = collector.GetSnapshot()
            .Where(r => r.Level == LogLevel.Warning)
            .Where(r => r.Message.Contains("mismatched ServiceControl container image versions"))
            .ToList();

        Assert.That(warnings, Is.Empty);
    }

    static DistributedApplicationBuilder CreatePublishBuilder()
    {
        var outDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName().Replace(".", ""));

        var builder = new DistributedApplicationBuilder(new DistributedApplicationOptions
        {
            Args = ["--operation", "publish", "--step", "publish", "--output-path", outDir],
            DisableDashboard = true
        });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Particular:AllowLearningTransportPublish"] = "true",
            ["ASPIRE_DCP_PATH"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aspire/bin/dcp"),
            ["ASPIRE_DASHBOARD_PATH"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aspire/managed/wwwroot")
        });

        return builder;
    }
}
