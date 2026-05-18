namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using global::Aspire.Hosting;
using TestResources;
using Transport;

public class PublishPlatformDefaultsWithSqsTransportApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");

        builder
            .AddParticularPlatform("particular")
            .WithTransportAmazonSqs(builder.AddDummyConnectionString("transport-connection"))
            .AddDefaultComponents();
    }
}