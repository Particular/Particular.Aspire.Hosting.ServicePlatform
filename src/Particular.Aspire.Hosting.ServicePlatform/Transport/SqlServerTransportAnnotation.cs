namespace Particular.Aspire.Hosting.ServicePlatform.Transport;

using global::Aspire.Hosting.ApplicationModel;

class SqlServerTransportAnnotation(IResourceWithConnectionString connectionSource, string? queueSchema, string? subscriptionsTable) : PlatformTransportAnnotation
{
    public override string TransportType => "SQLServer";
    public override IResourceWithConnectionString ConnectionSource { get; } = connectionSource;

    public string? QueueSchema { get; } = queueSchema;

    public string? SubscriptionsTable { get; } = subscriptionsTable;

    protected override ReferenceExpression ServiceControlConnectionString
    {
        get
        {
            var builder = new ReferenceExpressionBuilder();
            builder.Append($"{ConnectionSource.ConnectionStringExpression}");
            builder.AppendKeyword("Queue Schema", QueueSchema);
            builder.AppendKeyword("Subscriptions Table", SubscriptionsTable);
            return builder.Build();
        }
    }
}