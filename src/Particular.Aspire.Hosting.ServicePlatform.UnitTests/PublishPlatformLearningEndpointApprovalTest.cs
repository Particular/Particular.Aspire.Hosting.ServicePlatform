namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using Tests;

public class PublishPlatformLearningEndpointApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddDockerComposeEnvironment("compose");
        var platform = builder
            .AddParticularPlatform("particular")
            .AddDefaultComponents();

        builder.AddContainer("endpoint", "endpoint-container")
            .WithParticularPlatform(platform);
    }
}