[![](https://img.shields.io/nuget/v/soenneker.testhosts.integration.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.testhosts.integration/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.testhosts.integration/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.testhosts.integration/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.testhosts.integration.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.testhosts.integration/)

# Soenneker.TestHosts.Integration

Provides a reusable and generic integration test host that dynamically registers and configures `WebApplicationFactory{TEntryPoint}` instances for multiple ASP.NET Core projects, with support for custom app settings, authentication, logging, and test utilities.

## Install

```bash
dotnet add package Soenneker.TestHosts.Integration
```

## Quick start

```csharp
using Soenneker.TestHosts.Integration.Abstract;

IIntegrationTestHost integrationTestHost = /* resolve from DI */;
integrationTestHost.RegisterFactory("value");
```

Registers a lazy `WebApplicationFactory{TEntryPoint}` for the specified startup type and project name.

## What you get

- `IIntegrationTestHost` — Provides a reusable and generic integration test host that dynamically registers and configures `WebApplicationFactory{TEntryPoint}` instances for multiple ASP.NET Core projects, with support for custom app settings, authentication, logging, and test utilities.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `IIntegrationTestHost.Faker` | A configured instance of `Faker` for generating random data in tests. | A configured instance of `Faker` for generating random data in tests. |
| `IIntegrationTestHost.AutoFaker` | A configured instance of `AutoFaker` using optional custom configuration. | A configured instance of `AutoFaker` using optional custom configuration. |

## Important behavior

- `IIntegrationTestHost.GetFactory()`: Thrown if the factory was not registered first.

## Practical notes

- Dispose instances you own when their scope ends so held resources can be released.
