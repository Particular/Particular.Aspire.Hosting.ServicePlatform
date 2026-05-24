namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using System;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Platform;
using Transport;

/// <summary>
/// Configures throughput reporting settings for servicecontrol when using RabbitMQ.
/// https://docs.particular.net/servicecontrol/servicecontrol-instances/configuration#usage-reporting-when-using-the-rabbitmq-transport
/// </summary>
public sealed class ThroughputReportingRabbitMq : IThroughputReportingProvider
{
    readonly IExpressionValue? apiUrl;
    readonly IExpressionValue? userName;
    readonly IExpressionValue? password;

    /// <summary>
    /// Configures throughput reporting settings for servicecontrol when using RabbitMQ.
    /// https://docs.particular.net/servicecontrol/servicecontrol-instances/configuration#usage-reporting-when-using-the-rabbitmq-transport
    /// </summary>
    /// <param name="apiUrl">The RabbitMQ management URL.</param>
    /// <param name="userName">The username to access the RabbitMQ management interface.</param>
    /// <param name="password">The password to access the RabbitMQ management interface.</param>
    public ThroughputReportingRabbitMq(IExpressionValue? apiUrl, IExpressionValue? userName, IExpressionValue? password)
    {
        this.apiUrl = apiUrl;
        this.userName = userName;
        this.password = password;
    }


    /// <inheritdoc />
    public void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        ArgumentNullException.ThrowIfNull(errorInstance);

        if (errorInstance.Resource.Parent.GetTransport() is not RabbitMqTransportAnnotation)
        {
            throw new InvalidOperationException(
                $"{nameof(ThroughputReportingRabbitMq)} requires the parent platform to be configured with WithTransportRabbitMq first.");
        }

        if (apiUrl is not null)
        {
            errorInstance.WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.RabbitMq.ApiUrl, apiUrl);
        }

        if (userName is not null)
        {
            errorInstance.WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.RabbitMq.Username, userName);
        }

        if (password is not null)
        {
            errorInstance.WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.RabbitMq.Password, password);
        }
    }
}