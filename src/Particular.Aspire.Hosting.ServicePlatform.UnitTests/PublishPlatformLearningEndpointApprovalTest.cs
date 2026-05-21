namespace Particular.Aspire.Hosting.ServicePlatform.Tests;

using global::Aspire.Hosting;

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