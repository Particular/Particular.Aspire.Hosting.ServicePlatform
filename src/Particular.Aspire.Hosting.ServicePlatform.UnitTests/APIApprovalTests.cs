namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests;

using global::Aspire.Hosting;
using NUnit.Framework;
using Particular.Approvals;
using PublicApiGenerator;

[TestFixture]
public class APIApprovals
{
    [Test]
    public void ApproveHostingApi()
    {
        var publicApi = typeof(ParticularPlatformExtensions).Assembly
            .GeneratePublicApi(new ApiGeneratorOptions
            {
                ExcludeAttributes =
                [
                    "System.Runtime.Versioning.TargetFrameworkAttribute",
                    "System.Reflection.AssemblyMetadataAttribute"
                ]
            });
        Approver.Verify(publicApi);
    }
}