namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using System;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

/// <summary>
/// Configures throughput reporting for Azure Service Bus by supplying the Azure AD credentials
/// and subscription details needed for ServiceControl to query Service Bus management APIs.
/// </summary>
public sealed class ThroughputReportingAzureServiceBus : IThroughputReportingProvider
{
    readonly IExpressionValue? serviceBusName;
    readonly IExpressionValue tenantId;
    readonly IExpressionValue subscriptionId;
    readonly IExpressionValue clientId;
    readonly IExpressionValue clientSecret;
    readonly IExpressionValue? managementUrl;

    /// <summary>
    /// Creates a new Azure Service Bus throughput reporting provider.
    /// </summary>
    /// <param name="tenantId">The Azure AD tenant ID.</param>
    /// <param name="subscriptionId">The Azure subscription ID containing the Service Bus namespace.</param>
    /// <param name="clientId">The Azure AD application (client) ID.</param>
    /// <param name="clientSecret">The Azure AD application client secret.</param>
    /// <param name="serviceBusName">The Service Bus namespace name. If not provided, it is inferred from the transport configuration.</param>
    /// <param name="managementUrl">An optional custom Azure management URL, for use with sovereign clouds.</param>
    public ThroughputReportingAzureServiceBus(
        IExpressionValue tenantId,
        IExpressionValue subscriptionId,
        IExpressionValue clientId,
        IExpressionValue clientSecret,
        IExpressionValue? serviceBusName = null,
        IExpressionValue? managementUrl = null)
    {
        ArgumentNullException.ThrowIfNull(tenantId);
        ArgumentNullException.ThrowIfNull(subscriptionId);
        ArgumentNullException.ThrowIfNull(clientId);
        ArgumentNullException.ThrowIfNull(clientSecret);

        this.tenantId = tenantId;
        this.subscriptionId = subscriptionId;
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        this.serviceBusName = serviceBusName;
        this.managementUrl = managementUrl;
    }

    /// <inheritdoc />
    public void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        ArgumentNullException.ThrowIfNull(errorInstance);

        if (errorInstance.Resource.Parent.GetTransport() is not AzureServiceBusTransportAnnotation)
        {
            throw new InvalidOperationException(
                $"{nameof(ThroughputReportingAzureServiceBus)} requires the parent platform to be configured with WithTransportAzureServiceBus first.");
        }

        errorInstance
            .WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AzureServiceBus.TenantId, tenantId)
            .WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AzureServiceBus.SubscriptionId, subscriptionId)
            .WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AzureServiceBus.ClientId, clientId)
            .WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AzureServiceBus.ClientSecret, clientSecret);

        if (serviceBusName is not null)
        {
            errorInstance.WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AzureServiceBus.ServiceBusName, serviceBusName);
        }

        if (managementUrl is not null)
        {
            errorInstance.WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AzureServiceBus.ManagementUrl, managementUrl);
        }
    }
}
