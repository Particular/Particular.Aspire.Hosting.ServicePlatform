# Aspire Custom Integration Guide

A short reference for building custom .NET Aspire hosting integrations. Verified against Aspire 13.x APIs (Aspire.Hosting v13.1.0). For the full API reference, see [Microsoft Learn: Aspire](https://learn.microsoft.com/dotnet/aspire/) and the [Create custom hosting integrations](https://learn.microsoft.com/dotnet/aspire/extensibility/custom-hosting-integration) tutorial. Source links to specific APIs are inline below; a complete sources list is at the bottom.

## Project layout

Split hosting and client into separate packages:

```text
MyIntegration.Hosting/    # Aspire.Hosting reference: resource types + AddXxx extensions
MyIntegration.Client/     # Microsoft.Extensions.Hosting reference: AddXxxClient extensions
MyIntegration.AppHost/    # references MyIntegration.Hosting
```

When the AppHost references your hosting library, add `IsAspireProjectResource="false"`. Otherwise Aspire treats it as a service project and emits [ASPIRE004](https://learn.microsoft.com/dotnet/aspire/diagnostics/aspire004):

```xml
<ProjectReference Include="..\MyIntegration.Hosting\MyIntegration.Hosting.csproj"
                  IsAspireProjectResource="false" />
```

See the [Create custom hosting integrations](https://learn.microsoft.com/dotnet/aspire/extensibility/custom-hosting-integration) tutorial for the canonical example.

## Naming conventions

| Element          | Namespace                         | Shape                                                               |
| ---------------- | --------------------------------- | ------------------------------------------------------------------- |
| Resource type    | `Aspire.Hosting.ApplicationModel` | `FooResource : ContainerResource`                                   |
| Extension class  | `Aspire.Hosting`                  | `FooResourceBuilderExtensions`                                      |
| `Add*` method    | `Aspire.Hosting`                  | `AddFoo(this IDistributedApplicationBuilder, string name)`          |
| `With*` method   | `Aspire.Hosting`                  | `WithBar(this IResourceBuilder<FooResource>, ...)`                  |
| Client extension | `Microsoft.Extensions.Hosting`    | `AddFooClient(this IHostApplicationBuilder, string connectionName)` |
| Annotation       | anywhere                          | `FooAnnotation : IResourceAnnotation`                               |

Putting `Add*`/`With*` in `Aspire.Hosting` makes them discoverable without extra `using` statements.

## Resource type design

Keep resource classes minimal. Derive from the most specific base:

- Container → `ContainerResource`
- Executable/CLI → `ExecutableResource`
- Synthetic grouping / config-only → `Resource`

Implement marker interfaces as needed:

- `IResourceWithConnectionString`: if consumers will `.WithReference(yours)`
- `IResourceWithParent<TParent>`: if the child's lifecycle is truly coupled to a parent (dies when parent dies)
- `IResourceWithWaitSupport`: if your resource needs to call `.WaitFor(other)` itself (already implemented by `ContainerResource`, `ExecutableResource`, and `ProjectResource`, so most custom resources don't need to add it)

Decorate the name parameter with `[ResourceName]` for analyzer validation.

```csharp
namespace Aspire.Hosting.ApplicationModel;

public sealed class FooResource([ResourceName] string name)
    : ContainerResource(name), IResourceWithConnectionString
{
    internal const string PrimaryEndpointName = "http";

    private EndpointReference? _primaryEndpoint;
    public EndpointReference PrimaryEndpoint => _primaryEndpoint ??= new(this, PrimaryEndpointName);

    public ReferenceExpression ConnectionStringExpression =>
        ReferenceExpression.Create($"http://{PrimaryEndpoint.Property(EndpointProperty.HostAndPort)}");
}
```

**Why minimal?** Resources are effectively a discriminated union of annotations. Putting state on derived classes hides it from the dashboard, manifest publisher, and test infrastructure, which all walk `resource.Annotations`.

## Connection strings

Use `ReferenceExpression.Create` with an interpolated template. Ports and hostnames aren't known until runtime, so plain strings evaluate too early:

```csharp
public ReferenceExpression ConnectionStringExpression =>
    ReferenceExpression.Create(
        $"Host={Endpoint.Property(EndpointProperty.Host)};Port={Endpoint.Property(EndpointProperty.Port)}");
```

For secrets, declare them via `builder.AddParameter("password", secret: true)` and reference the resulting `ParameterResource` inside the expression. Never embed raw secrets in the template, or they'll leak into the published manifest. Example:

```csharp
var password = builder.AddParameter("password", secret: true);
// ...
public ReferenceExpression ConnectionStringExpression =>
    ReferenceExpression.Create(
        $"Host={Endpoint.Property(EndpointProperty.Host)};Password={password.Resource}");
```

## Parent/child relationships

Two orthogonal concerns:

| Method                                  | Effect                                 | Use for                                   |
| --------------------------------------- | -------------------------------------- | ----------------------------------------- |
| `.WithParentRelationship(parent)`       | Dashboard nesting only                 | Logical grouping                          |
| `IResourceWithParent<TParent>` on child | Dashboard nesting + lifecycle coupling | True containment (child dies with parent) |

**For `.WaitFor` / startup ordering:**

| Method                    | Waits for                      | Use when                                        |
| ------------------------- | ------------------------------ | ----------------------------------------------- |
| `.WaitFor(dep)`           | `Running` + health checks pass | Long-running services (DBs, queues)             |
| `.WaitForStart(dep)`      | `Running` only                 | Dependency's health check depends on the waiter |
| `.WaitForCompletion(dep)` | `Finished` / `Exited`          | One-shot setup (migrations, seeders)            |

`WaitFor` only adds a health-check gate when the dependency has a `HealthCheckAnnotation`; without one it just waits for `Running`, which usually isn't enough for real readiness. Attach a health check via `.WithHealthCheck(...)`. See [`WaitFor` remarks on Microsoft Learn](https://learn.microsoft.com/dotnet/api/aspire.hosting.resourcebuilderextensions.waitfor) for the exact gating semantics.

## Annotations are the extensibility substrate

Most `.WithXxx(...)` methods just attach an annotation to the resource. To add custom configuration, define your own:

```csharp
public sealed class FooConfigAnnotation(string setting, int value) : IResourceAnnotation
{
    public string Setting { get; } = setting;
    public int Value { get; } = value;
}

public static IResourceBuilder<T> WithFooConfig<T>(
    this IResourceBuilder<T> builder, string setting, int value)
    where T : FooResource
    => builder.WithAnnotation(new FooConfigAnnotation(setting, value));
```

Built-in annotations to know: `EndpointAnnotation`, `EnvironmentCallbackAnnotation`, `ContainerImageAnnotation`, `ContainerMountAnnotation`, `ResourceRelationshipAnnotation`, `ResourceCommandAnnotation`, `ResourceUrlAnnotation`, `HealthCheckAnnotation`, `WaitAnnotation`.

## Lifecycle: use eventing, not hooks

[`IDistributedApplicationLifecycleHook`](https://learn.microsoft.com/dotnet/api/aspire.hosting.lifecycle.idistributedapplicationlifecyclehook) and [`AddLifecycleHook<T>`](https://learn.microsoft.com/dotnet/api/aspire.hosting.lifecycle.lifecyclehookservicecollectionextensions.addlifecyclehook) are **obsolete** in Aspire 13.x. Use `IDistributedApplicationEventingSubscriber` and `builder.Services.AddEventingSubscriber<T>()` (or `TryAddEventingSubscriber<T>()`):

```csharp
internal sealed class MySubscriber(ResourceNotificationService notifications)
    : IDistributedApplicationEventingSubscriber
{
    public Task SubscribeAsync(
        IDistributedApplicationEventing eventing,
        DistributedApplicationExecutionContext executionContext,
        CancellationToken cancellationToken)
    {
        eventing.Subscribe<BeforeStartEvent>(OnBeforeStart);
        eventing.Subscribe<ResourceReadyEvent>(OnResourceReady);
        return Task.CompletedTask;
    }
    // ...
}
```

Events split into two categories. Pick the right `Subscribe` overload:

- **Global** (`IDistributedApplicationEvent`), subscribe via `eventing.Subscribe<T>(callback)`:
  - `BeforeStartEvent`: published before the application starts.
  - `AfterResourcesCreatedEvent`: published after all resources have been created.
- **Resource-scoped** (`IDistributedApplicationResourceEvent`), subscribe via `eventing.Subscribe<T>(resource, callback)`:
  - `BeforeResourceStartedEvent`: orchestrator is about to start a specific resource.
  - `ResourceEndpointsAllocatedEvent`: endpoints for a specific resource are allocated; `ResourceUrlsCallbackAnnotation` callbacks fire here.
  - `ResourceReadyEvent`: resource first reaches a ready state.
  - `ResourceStoppedEvent`: resource has stopped.

[`AfterEndpointsAllocatedEvent` is obsolete](https://learn.microsoft.com/dotnet/api/aspire.hosting.applicationmodel.afterendpointsallocatedevent). Use `BeforeResourceStartedEvent` (per-resource, before start) or `ResourceEndpointsAllocatedEvent` (per-resource, after endpoints allocated) depending on what you need.

## Synthetic parent resources

For grouping-only nodes (no process), always:

```csharp
var parent = builder.AddResource(new MyGroupResource(name))
    .WithInitialState(new CustomResourceSnapshot
    {
        ResourceType = "MyGroup",
        State = new ResourceStateSnapshot(KnownResourceStates.Starting, KnownResourceStateStyles.Info),
        Properties = []
    })
    .ExcludeFromManifest();
```

Without `WithInitialState` the resource sits in an undefined dashboard state forever. Without `ExcludeFromManifest` it leaks into deployment output. `CustomResourceSnapshot.State` is typed `ResourceStateSnapshot?` but has an implicit `string` conversion, so `State = KnownResourceStates.Running` also works.

To hide a resource, use `snapshot with { IsHidden = true }`. [`KnownResourceStates.Hidden` is obsolete](https://learn.microsoft.com/dotnet/api/aspire.hosting.applicationmodel.knownresourcestates.hidden).

## State updates

`ResourceNotificationService` drives all resource state:

```csharp
// Watch changes
await foreach (var evt in notifications.WatchAsync(ct)) { ... }

// Publish updates
await notifications.PublishUpdateAsync(resource, s => s with
{
    State = new ResourceStateSnapshot(KnownResourceStates.Running, KnownResourceStateStyles.Success)
});

// Wait for a specific state
await notifications.WaitForResourceHealthyAsync(name, ct);
```

## Anti-patterns to avoid

| Anti-pattern                                                          | Why it's wrong                                                     | Right pattern                                                                              |
| --------------------------------------------------------------------- | ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------ |
| Store `IResource` child references as properties on a parent          | Duplicates annotations, invisible to tooling                       | Query app model via `ResourceRelationshipAnnotation`                                       |
| Store `IResourceBuilder<T>` on a resource class                       | Builders are configuration-time views; resources are runtime state | Return builder from extension; use `CreateResourceBuilder` when needed                     |
| Return a custom "compound builder" from `AddXxx`                      | Breaks `IResourceBuilder<T>` contract                              | Return `IResourceBuilder<TParent>`, expose children via chained extensions or `out` params |
| `IResourceWithParent<T>` for visual grouping                          | Couples lifecycles; child dies with parent                         | Use `WithParentRelationship` for visual-only                                               |
| Plain string interpolation for connection strings                     | Evaluates too early; ports unknown                                 | `ReferenceExpression.Create($"...")`                                                       |
| Secrets in connection string template                                 | Leaks into manifest                                                | Use `ParameterResource`                                                                    |
| Mutable `{ get; set; }` properties on resources for user config       | Invisible to tooling                                               | Custom `IResourceAnnotation` + `WithXxx` extension                                         |
| `WaitFor` without a health check on the dependency                    | Waits only for `Running`, not readiness                            | Provide a health check, or use `WaitForStart` explicitly                                   |
| Fire-and-forget `_ = DoWork(ct)` in subscribers                       | Leaks past shutdown, no error observability                        | Store task in field + `CancellationTokenSource` + `IAsyncDisposable`                       |
| Manually publishing `ResourceReadyEvent`                              | Aspire manages this based on health checks                         | Provide a health check instead                                                             |
| [`AddLifecycleHook<T>()`](https://learn.microsoft.com/dotnet/api/aspire.hosting.lifecycle.lifecyclehookservicecollectionextensions.addlifecyclehook) in new code | Marked `[Obsolete]`                                                | [`AddEventingSubscriber<T>()`](https://learn.microsoft.com/dotnet/api/aspire.hosting.lifecycle.eventingsubscriberservicecollectionextensions.addeventingsubscriber) |
| [`KnownResourceStates.Hidden`](https://learn.microsoft.com/dotnet/api/aspire.hosting.applicationmodel.knownresourcestates.hidden) to hide resources | Marked `[Obsolete]`                                                | `snapshot with { IsHidden = true }`                                                        |
| Missing `IsAspireProjectResource="false"` on hosting ProjectReference | AppHost treats library as a service project                        | Add the attribute in the csproj                                                            |
| Forgetting `ExcludeFromManifest` on synthetic parents                 | Dev-only resources leak into deployment                            | Always exclude grouping/synthetic nodes                                                    |
| `FirstOrDefault` on annotations where multiple may exist              | Silently loses information                                         | `.OfType<T>()` iterate, or `LastOrDefault` for "last wins"                                 |

## Mental model

Aspire resources are a **discriminated union of annotations**: composable, queryable, discoverable uniformly across the dashboard, manifest publisher, testing tools, and orchestrator. Every design choice should reinforce that:

- Resource classes are identity + connection string shape, little else.
- Behavior and config live on annotations.
- Builders are configuration-time views. Don't store them. Don't extend their lifetime past `Build()`.
- Keep configuration-time and runtime separated. `IResourceBuilder<T>` is config-time, `IResource` is runtime.
- Encode internal dependencies internally so consumers can drop in your integration and have it "just work."
