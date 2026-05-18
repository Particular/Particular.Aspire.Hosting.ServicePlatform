namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using global::Aspire.Hosting;
using Particular.Aspire.Hosting.ServicePlatform.Tests.TestResources;
using Particular.Aspire.Hosting.ServicePlatform.Transport;

public class PublishPlatformDefaultsWithEndpointApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAzureServiceBus(builder.AddDummyConnectionString("transport-connection"))
            .AddDefaultComponents();

        builder.AddContainer("endpoint", "endpoint_image")
            .WithParticularPlatform(platform);
    }
}