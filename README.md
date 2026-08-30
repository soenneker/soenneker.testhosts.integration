[![](https://img.shields.io/nuget/v/soenneker.testhosts.integration.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.testhosts.integration/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.testhosts.integration/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.testhosts.integration/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.testhosts.integration.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.testhosts.integration/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.testhosts.integration/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.testhosts.integration/actions/workflows/codeql.yml)

# Soenneker.TestHosts.Integration

A TUnit-compatible host for lazily creating and sharing configured ASP.NET Core `WebApplicationFactory` instances.

## Installation

```bash
dotnet add package Soenneker.TestHosts.Integration
```

## Define the shared host

```csharp
using Soenneker.TestHosts.Integration;

public sealed class Host : IntegrationTestHost
{
    public override Task InitializeAsync()
    {
        RegisterFactory<Api.Program>("Api");
        return base.InitializeAsync();
    }
}
```

Call `RegisterFactory<TEntryPoint>` before requesting that type. Registration is lazy: the application is not built until the factory's `Value` is first used. Registering the same entry-point type again has no effect.

## Use it from TUnit

```csharp
using TUnit.Core;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class HealthTests
{
    private readonly Host _host;

    public HealthTests(Host host)
    {
        _host = host;
    }

    [Test]
    public async Task Health_is_available()
    {
        var factory = _host.GetFactory<Api.Program>().Value;
        using HttpClient client = factory.CreateClient();

        using HttpResponseMessage response = await client.GetAsync("/health");
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }
}
```

TUnit initializes the host's Bogus `Faker` and `AutoFaker` and disposes every factory that was actually created. The host itself owns those factories; tests should dispose clients and responses, not the shared factory.

## Test-host configuration

For each registered project name, the host requires an `appsettings.json` at the path returned by `IntegrationTestHost.GetAppSettingsPath(projectName)`. That path is `<parent of AppContext.BaseDirectory>/<projectName>/appsettings.json`; missing files fail factory creation.

Each factory also installs the test authentication scheme, JWT utilities, loopback-address middleware, and a Serilog sink for the active TUnit context. The loopback rewrite is test-only and should not be copied into production startup.

`GetFactory<TEntryPoint>()` returns the registered `Lazy<WebApplicationFactory<TEntryPoint>>` and throws when that entry point was never registered.
