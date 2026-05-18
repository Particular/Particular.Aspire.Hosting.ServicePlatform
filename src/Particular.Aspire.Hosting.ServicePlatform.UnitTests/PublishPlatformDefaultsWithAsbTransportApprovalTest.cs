namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using global::Aspire.Hosting;
using Particular.Aspire.Hosting.ServicePlatform.Tests.TestResources;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

public class PublishPlatformDefaultsWithAsbTransportApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        builder
            .AddParticularPlatform("particular")
            .WithTransportAzureServiceBus(builder.AddDummyConnectionString("transport-connection"))
            .AddDefaultComponents();
    }
}