namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using TestResources;

public class PublishPlatformDefaultsWithMultipleEndpointsApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        var platform = builder
            .AddParticularPlatform("particular")
            .WithTransportAzureServiceBus(builder.AddDummyConnectionString("transport-connection"))
            .AddDefaultComponents();

        builder.AddContainer("endpoint-one", "endpoint_image_one")
            .WithParticularPlatform(platform);

        builder.AddContainer("endpoint-two", "endpoint_image_two")
            .WithParticularPlatform(platform);
    }
}
