namespace Particular.Aspire.Hosting.ServicePlatform.ThroughputReporting;

using System;
using global::Aspire.Hosting;
using global::Aspire.Hosting.ApplicationModel;
using Platform;
using Transport;

/// <summary>
/// This is used to configure the usage reporting component in Service Control as described
/// at https://docs.particular.net/servicecontrol/servicecontrol-instances/configuration?version=servicecontrol_4#usage-reporting-when-using-the-amazon-sqs-transport
/// </summary>
public sealed class ThroughputReportingAmazonSqs : IThroughputReportingProvider
{
    internal const string AccessKeyEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_ACCESSKEY";
    internal const string SecretAccessKeyEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_SECRETKEY";
    internal const string ProfileEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_PROFILE";
    internal const string RegionEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_REGION";
    internal const string QueueNamePrefixEnvVar = "LICENSINGCOMPONENT_AMAZONSQS_PREFIX";

    readonly ReferenceExpression? accessKeyId;
    readonly ReferenceExpression? secretKey;
    readonly ReferenceExpression? profile;
    readonly ReferenceExpression? region;
    readonly ReferenceExpression? prefix;

    public ThroughputReportingAmazonSqs(
        ReferenceExpression? accessKeyId = null,
        ReferenceExpression? secretKey = null,
        ReferenceExpression? region = null,
        ReferenceExpression? profile = null,
        ReferenceExpression? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(accessKeyId);
        ArgumentNullException.ThrowIfNull(secretKey);
        ArgumentNullException.ThrowIfNull(region);

        this.accessKeyId = accessKeyId;
        this.secretKey = secretKey;
        this.region = region;
        this.profile = profile;
        this.prefix = prefix;
    }

    public void ApplyTo(IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance)
    {
        ArgumentNullException.ThrowIfNull(errorInstance);

        if (errorInstance.Resource.Parent.GetTransport() is not AmazonSqsTransportAnnotation)
        {
            throw new InvalidOperationException(
                $"{nameof(ThroughputReportingAmazonSqs)} requires the parent platform to be configured with WithTransportAmazonSqs first.");
        }

        if (accessKeyId != null)
        {
            errorInstance.WithEnvironment(AccessKeyEnvVar, accessKeyId);
        }

        if (secretKey != null)
        {
            errorInstance.WithEnvironment(SecretAccessKeyEnvVar, secretKey);
        }

        if (region != null)
        {
            errorInstance.WithEnvironment(RegionEnvVar, region);
        }

        if (prefix is not null)
        {
            errorInstance.WithEnvironment(QueueNamePrefixEnvVar, prefix);
        }

        if (profile is not null)
        {
            errorInstance.WithEnvironment(ProfileEnvVar, profile);
        }
    }
}
