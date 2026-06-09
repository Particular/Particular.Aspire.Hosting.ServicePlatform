namespace Particular.Aspire.Hosting.ServicePlatform.UnitTests.TestResources;

using global::Aspire.Hosting.ApplicationModel;

public class DummyConnectionStringResource(string name, string? value = null)
    : Resource(name), IResourceWithConnectionString
{
    public ReferenceExpression ConnectionStringExpression { get; } = value != null
        ? ReferenceExpression.Create($"{value}")
        : ReferenceExpression.Create($"Name={name}");
}
