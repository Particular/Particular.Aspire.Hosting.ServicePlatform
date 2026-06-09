namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;

public class PublishPlatformDefaultsAzureApprovalTest : AspireApplicationPublishingTestBase
{
    protected override void BuildApplication(IDistributedApplicationBuilder builder)
    {
        builder.AddAzureContainerAppEnvironment("acr");
        builder
            .AddParticularPlatform("particular")
            .AddDefaultComponents();
    }
}