namespace Particular.Aspire.Hosting.ServicePlatform.Tests.TestResources;

using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;

public static class TestResourceExtensions
{
    extension(IDistributedApplicationBuilder builder)
    {
        /// <summary>
        /// Like AddConnectionString but allows you to provide a value instead
        /// of having it come from config/env vars or params, useful when a resource requires
        /// a connection string in a test, but you are not adding a resource to provide one.
        /// </summary>
        public IResourceBuilder<IResourceWithConnectionString> AddDummyConnectionString(
            string name,
            string? value = null)
        {
            return builder.AddResource(new DummyConnectionStringResource(name, value));
        }
    }
}