# Particular.Aspire.Hosting.ServicePlatform

Particular.Aspire.Hosting.ServicePlatform enables the use of [Aspire](https://aspire.dev/) service as the underlying transport used by NServiceBus. This packages uses the [Aspire.Hosting package](https://www.nuget.org/packages/Aspire.Hosting/).

It is part of the [Particular Service Platform](https://particular.net/service-platform), which includes [NServiceBus](https://particular.net/nservicebus) and tools to build, monitor, and debug distributed systems.

See the [Aspire documentation](https://docs.particular.net/shape-the-future/aspire) for more details on how to use it.

## How to Test Locally

The tests in `src/Particular.Aspire.Hosting.ServicePlatform.UnitTests` are approval tests that exercise the Aspire `publish` operation and verify the generated manifest. They do not start any runtime services, so no databases, message brokers, or containers need to be running on the host machine.

### Required infrastructure

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — the version pinned in [`global.json`](global.json).

No connection string configuration or Docker container is required to run the tests.

### Running the tests

From the repository root:

```sh
dotnet test src/Particular.Aspire.Hosting.ServicePlatform.slnx
```
