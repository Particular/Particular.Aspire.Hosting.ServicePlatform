namespace Particular.Aspire.Hosting.ServicePlatform;

/// <summary>
/// This is a partial list of configuration values for the Particular Service Platform and
/// represents the list of environment variables that are used by this integration to configure the platform.
/// </summary>
static class PlatformEnvironment
{
    public const string ParticularSoftwareLicense = "PARTICULARSOFTWARE_LICENSE";
    public const string RavenDbConnectionString = "RAVENDB_CONNECTIONSTRING";

    public static class ServiceControl
    {
        public const string ConnectionString = "CONNECTIONSTRING";
        public const string TransportType = "TRANSPORTTYPE";
        public const string ErrorQueue = "SERVICEBUS_ERRORQUEUE";
        public const string AuditQueue = "SERVICEBUS_AUDITQUEUE";
        public const string RemoteInstances = "REMOTEINSTANCES";

        public static class LicensingComponent
        {
            public const string ServiceControlThroughputDataQueue = "LICENSINGCOMPONENT_SERVICECONTROLTHROUGHPUTDATAQUEUE";

            public static class AzureServiceBus
            {
                public const string ServiceBusName = "LICENSINGCOMPONENT_ASB_SERVICEBUSNAME";
                public const string TenantId = "LICENSINGCOMPONENT_ASB_TENANTID";
                public const string SubscriptionId = "LICENSINGCOMPONENT_ASB_SUBSCRIPTIONID";
                public const string ClientId = "LICENSINGCOMPONENT_ASB_CLIENTID";
                public const string ClientSecret = "LICENSINGCOMPONENT_ASB_CLIENTSECRET";
                public const string ManagementUrl = "LICENSINGCOMPONENT_ASB_MANAGEMENTURL";
            }

            public static class RabbitMq
            {
                public const string ApiUrl = "LICENSINGCOMPONENT_RABBITMQ_APIURL";
                public const string Username = "LICENSINGCOMPONENT_RABBITMQ_USERNAME";
                public const string Password = "LICENSINGCOMPONENT_RABBITMQ_PASSWORD";
            }

            public static class AmazonSqs
            {
                public const string AccessKey = "LICENSINGCOMPONENT_AMAZONSQS_ACCESSKEY";
                public const string SecretKey = "LICENSINGCOMPONENT_AMAZONSQS_SECRETKEY";
                public const string Profile = "LICENSINGCOMPONENT_AMAZONSQS_PROFILE";
                public const string Region = "LICENSINGCOMPONENT_AMAZONSQS_REGION";
                public const string Prefix = "LICENSINGCOMPONENT_AMAZONSQS_PREFIX";
            }

            public static class SqlServer
            {
                public const string ConnectionString = "LICENSINGCOMPONENT_SQLSERVER_CONNECTIONSTRING";
                public const string AdditionalCatalogs = "LICENSINGCOMPONENT_SQLSERVER_ADDITIONALCATALOGS";
            }
        }
    }

    public static class ServicePulse
    {
        public const string ServiceControlUrl = "SERVICECONTROL_URL";
        public const string MonitoringUrl = "MONITORING_URL";
    }

    public static class Monitoring
    {
        public const string InstanceName = "MONITORING_INSTANCENAME";
        public const string ServiceControlThroughputDataQueue = "MONITORING_SERVICECONTROLTHROUGHPUTDATAQUEUE";
    }

}