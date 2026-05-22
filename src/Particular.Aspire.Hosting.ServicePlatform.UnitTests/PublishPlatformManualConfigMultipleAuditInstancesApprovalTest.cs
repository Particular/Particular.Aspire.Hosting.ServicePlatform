namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using global::Aspire.Hosting;
using Particular.Aspire.Hosting.ServicePlatform.Tests.TestResources;

public class PublishPlatformManualConfigMultipleAuditInstancesApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAzureServiceBus(builder.AddDummyConnectionString("transport-connection"));

        var errorPersistence = platform.AddPersistenceRavenDb("error-persistence");
        var auditOnePersistence = platform.AddPersistenceRavenDb("audit-one-persistence");
        var auditTwoPersistence = platform.AddPersistenceRavenDb("audit-two-persistence");

        var error = platform.AddServiceControlErrorInstance("particular-error", errorPersistence);
        platform.AddServiceControlAuditInstance("particular-audit-one", error, auditOnePersistence);
        platform.AddServiceControlAuditInstance("particular-audit-two", error, auditTwoPersistence);

        var monitoring = platform.AddServiceControlMonitoringInstance("particular-monitoring");
        platform.AddServicePulse("particular-servicepulse", error, monitoring);
    }
}
