namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using Persistence;
using Tests;
using Tests.TestResources;
using Transport;

public class PublishPlatformDefaultsAzureStorageQueuesApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAzureStorageQueues(builder.AddDummyConnectionString("transport-connection"));

        platform.AddServiceControlErrorInstance("particular-error", platform.AddPersistenceRavenDb("particular-persistence"));

        platform.AddDefaultComponents();
    }
}
