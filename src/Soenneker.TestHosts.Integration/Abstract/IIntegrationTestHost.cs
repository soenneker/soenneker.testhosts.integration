using Bogus;
using Microsoft.AspNetCore.Mvc.Testing;
using Soenneker.Utils.AutoBogus;
using System;
using TUnit.Core.Interfaces;

namespace Soenneker.TestHosts.Integration.Abstract;

/// <summary>
/// Provides a reusable and generic integration test host that dynamically registers and configures
/// <see cref="WebApplicationFactory{TEntryPoint}"/> instances for multiple ASP.NET Core projects,
/// with support for custom app settings, authentication, logging, and test utilities.
/// </summary>
public interface IIntegrationTestHost : IAsyncInitializer, IAsyncDisposable
{
    /// <summary>
    /// A configured instance of <see cref="Faker"/> for generating random data in tests.
    /// </summary>
    Faker Faker { get; }

    /// <summary>
    /// A configured instance of <see cref="AutoFaker"/> using optional custom configuration.
    /// </summary>
    AutoFaker AutoFaker { get; }

    /// <summary>
    /// Registers a lazy <see cref="WebApplicationFactory{TEntryPoint}"/> for the specified startup type and project name.
    /// </summary>
    /// <typeparam name="TStartup">The startup class of the application under test.</typeparam>
    /// <param name="projectName">The name of the project containing the appsettings.json file.</param>
    void RegisterFactory<TStartup>(string projectName) where TStartup : class;

    /// <summary>
    /// Gets a registered lazy <see cref="WebApplicationFactory{TEntryPoint}"/> for the specified startup type.
    /// </summary>
    /// <typeparam name="TStartup">The startup class of the application under test.</typeparam>
    /// <returns>The registered <see cref="Lazy{T}"/> factory.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the factory was not registered first.</exception>
    Lazy<WebApplicationFactory<TStartup>> GetFactory<TStartup>() where TStartup : class;
}