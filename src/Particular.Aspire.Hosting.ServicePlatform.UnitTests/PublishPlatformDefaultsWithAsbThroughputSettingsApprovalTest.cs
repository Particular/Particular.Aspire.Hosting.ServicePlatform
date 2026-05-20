namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Tests.TestResources;
using Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;
using Tests;

public class PublishPlatformDefaultsWithAsbThroughputSettingsApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAzureServiceBus(builder.AddDummyConnectionString("transport-connection"));

        platform.AddServiceControlErrorInstance("particular-error", platform.AddPersistenceRavenDb("particular-persistence"))
            .WithThroughputReporting(new ThroughputReportingAzureServiceBus(
                builder.AddDummyConnectionString("throughput-connection", "connection-string-resource").Resource,
                ReferenceExpression.Create($"ref-expression"),
                builder.AddParameter("param", "parameter-value").Resource,
                ReferenceExpression.Create($"ref-expression-2")
            ));

        platform.AddDefaultComponents();
    }
}