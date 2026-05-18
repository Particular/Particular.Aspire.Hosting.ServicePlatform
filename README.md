# Particular.Aspire.Hosting.ServicePlatform

Particular.Aspire.Hosting.ServicePlatform enables the use of [Aspire](https://aspire.dev/) service as the underlying transport used by NServiceBus. This packages uses the [Aspire.Hosting package](https://www.nuget.org/packages/Aspire.Hosting/).

It is part of the [Particular Service Platform](https://particular.net/service-platform), which includes [NServiceBus](https://particular.net/nservicebus) and tools to build, monitor, and debug distributed systems.

See the [Aspire documentation](https://docs.particular.net/shape-the-future/aspire) for more details on how to use it.

## Installation

Add the package to your Aspire AppHost project:

```sh
dotnet add package Particular.Aspire.Hosting.ServicePlatform
```

## Quick start

Add the Particular Service Platform to your AppHost with sensible defaults:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var platform = builder
    .AddParticularPlatform("particular")
    .AddDefaultComponents();

builder.Build().Run();
```

`AddDefaultComponents()` wires up the Learning transport, a RavenDB persistence container, ServiceControl error/audit/monitoring instances, and ServicePulse.

To attach an NServiceBus endpoint to the platform so it picks up the configured transport and license:

```csharp
builder.AddProject<Projects.MyEndpoint>("my-endpoint")
    .WithParticularPlatform(platform);
```

For production use, swap the defaults for a real transport, persistence store, and license:

```csharp
var asb = builder.AddAzureServiceBus("asb");
var ravenDb = builder.AddRavenDB("ravendb");

builder
    .AddParticularPlatform("particular")
    .WithTransportAzureServiceBus(asb)
    .WithPersistenceRavenDb(ravenDb)
    .WithLicenseFromFile("license.xml")
    .AddDefaultComponents();
```

## Further reading

- [Particular Aspire documentation](https://docs.particular.net/shape-the-future/aspire) — official, externally hosted usage docs.

## How to Test Locally

The tests in `src/Particular.Aspire.Hosting.ServicePlatform.UnitTests` are approval tests that exercise the Aspire `publish` operation and verify the generated manifest. They do not start any runtime services, so no databases, message brokers, or containers need to be running on the host machine.

### Required infrastructure

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — the version pinned in [`global.json`](global.json).

No connection string configuration or Docker container is required to run the tests.

### Building and running the tests

From the repository root:

```sh
dotnet build src/Particular.Aspire.Hosting.ServicePlatform.slnx
dotnet test  src/Particular.Aspire.Hosting.ServicePlatform.slnx
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Licensed under the [RPL-1.5](LICENSE.md). See also [SECURITY.md](SECURITY.md) for the security policy.
