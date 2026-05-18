namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using System;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public sealed class ThroughputReportingAzureServiceBus : IThroughputReportingProvider
{
    internal const string ServiceBusNameEnvVar = "LICENSINGCOMPONENT_ASB_SERVICEBUSNAME";
    internal const string TenantIdEnvVar = "LICENSINGCOMPONENT_ASB_TENANTID";
    internal const string SubscriptionIdEnvVar = "LICENSINGCOMPONENT_ASB_SUBSCRIPTIONID";
    internal const string ClientIdEnvVar = "LICENSINGCOMPONENT_ASB_CLIENTID";
    internal const string ClientSecretEnvVar = "LICENSINGCOMPONENT_ASB_CLIENTSECRET";
    internal const string ManagementUrlEnvVar = "LICENSINGCOMPONENT_ASB_MANAGEMENTURL";

    readonly ReferenceExpression? serviceBusName;
    readonly ReferenceExpression tenantId;
    readonly ReferenceExpression subscriptionId;
    readonly ReferenceExpression clientId;
    readonly ReferenceExpression clientSecret;
    readonly ReferenceExpression? managementUrl;

    public ThroughputReportingAzureServiceBus(
        ReferenceExpression tenantId,
        ReferenceExpression subscriptionId,
        ReferenceExpression clientId,
        ReferenceExpression clientSecret,
        ReferenceExpression? serviceBusName = null,
        ReferenceExpression? managementUrl = null)
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

    public void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        ArgumentNullException.ThrowIfNull(errorInstance);

        if (errorInstance.Resource.Parent.GetTransport() is not AzureServiceBusTransportAnnotation)
        {
            throw new InvalidOperationException(
                $"{nameof(ThroughputReportingAzureServiceBus)} requires the parent platform to be configured with WithTransportAzureServiceBus first.");
        }

        errorInstance
            .WithEnvironment(TenantIdEnvVar, tenantId)
            .WithEnvironment(SubscriptionIdEnvVar, subscriptionId)
            .WithEnvironment(ClientIdEnvVar, clientId)
            .WithEnvironment(ClientSecretEnvVar, clientSecret);

        if (serviceBusName is not null)
        {
            errorInstance.WithEnvironment(ServiceBusNameEnvVar, serviceBusName);
        }

        if (managementUrl is not null)
        {
            errorInstance.WithEnvironment(ManagementUrlEnvVar, managementUrl);
        }
    }
}
