# Particular.Aspire.Hosting.ServicePlatform

The `Particular.Aspire.Hosting.ServicePlatform` package is an [Aspire](https://aspire.dev/) hosting integration that runs the Particular Platform (the ServiceControl instances, ServicePulse, persistence, and message transport) as part of an Aspire AppHost. It is intended for developers and technical leads who run the platform locally during development and want the same AppHost to carry through to publish-mode deployments, without maintaining a separate set of infrastructure scripts.

See the [Aspire documentation](https://docs.particular.net/platform/aspire) for more details on how to use it.

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
