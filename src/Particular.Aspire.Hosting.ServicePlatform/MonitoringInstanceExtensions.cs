namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Particular.Aspire.Hosting.ServicePlatform.Platform;

public static class MonitoringInstanceExtensions
{
    extension(IResourceBuilder<ServiceControlMonitoringInstanceResource> monitoring)
    {
        public IResourceBuilder<ServiceControlMonitoringInstanceResource> WithThroughputQueueFrom(
            IResourceBuilder<ServiceControlErrorInstanceResource> errorInstance) =>
            monitoring.WithEnvironment(ctx =>
            {
                if (errorInstance.Resource.TryGetLastAnnotation<ThroughputQueueAnnotation>(out var throughput))
                {
                    ctx.EnvironmentVariables[ServiceControlMonitoringInstanceResource.ThroughputQueueEnvVar] = throughput.QueueName;
                }
            });

        //this would be for instances where the queue address in monitoring needs to differ slightly from what it is in error, for example if using MSMQ and the monitoring instance is running on a different machine, it needs to add the machine name to the queue address
        //https://docs.particular.net/servicecontrol/monitoring-instances/configuration#usage-reporting-monitoringservicecontrolthroughputdataqueue
        public IResourceBuilder<ServiceControlMonitoringInstanceResource> WithThroughputQueue(
            string queueName) =>
            monitoring.WithEnvironment(ctx =>
            {
                ctx.EnvironmentVariables[ServiceControlMonitoringInstanceResource.ThroughputQueueEnvVar] = queueName;
            });

        public IResourceBuilder<ServiceControlMonitoringInstanceResource> WithMonitoringQueueName(string queueName) =>
            monitoring
                .WithEnvironment(ServiceControlMonitoringInstanceResource.MonitoringQueueEnvVar, queueName);
    }
}
