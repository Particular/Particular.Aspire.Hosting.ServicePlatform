namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using System;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Platform;
using Transport;

/// <summary>
/// Configures throughput reporting settings for ServiceControl when using Amazon SQS.
/// https://docs.particular.net/servicecontrol/servicecontrol-instances/configuration#usage-reporting-when-using-the-amazon-sqs-transport
/// </summary>
public sealed class ThroughputReportingAmazonSqs(
    IExpressionValue? accessKey,
    IExpressionValue? secretKey,
    IExpressionValue? profile,
    IExpressionValue? region,
    IExpressionValue? prefix) : IThroughputReportingProvider
{
    /// <inheritdoc />
    public void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        ArgumentNullException.ThrowIfNull(errorInstance);

        if (errorInstance.Resource.Parent.GetTransport() is not AmazonSqsTransportAnnotation)
        {
            throw new InvalidOperationException(
                $"{nameof(ThroughputReportingAmazonSqs)} requires the parent platform to be configured with WithTransportAmazonSqs first.");
        }

        if (accessKey is not null)
        {
            errorInstance.WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AmazonSqs.AccessKey, accessKey);
        }

        if (secretKey is not null)
        {
            errorInstance.WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AmazonSqs.SecretKey, secretKey);
        }

        if (profile is not null)
        {
            errorInstance.WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AmazonSqs.Profile, profile);
        }

        if (region is not null)
        {
            errorInstance.WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AmazonSqs.Region, region);
        }

        if (prefix is not null)
        {
            errorInstance.WithEnvironment(PlatformEnvironment.ServiceControl.LicensingComponent.AmazonSqs.Prefix, prefix);
        }
    }
}


