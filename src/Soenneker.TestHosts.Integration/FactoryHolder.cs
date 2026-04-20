using Microsoft.AspNetCore.Mvc.Testing;
using Soenneker.TestHosts.Integration.Abstract;
using System;
using System.Threading.Tasks;

namespace Soenneker.TestHosts.Integration;

/// <inheritdoc cref="IFactoryHolder"/>
internal sealed class FactoryHolder<TStartup> : IFactoryHolder where TStartup : class
{
    public Lazy<WebApplicationFactory<TStartup>> Factory { get; }

    public FactoryHolder(string projectName)
    {
        Factory = new Lazy<WebApplicationFactory<TStartup>>(
            () => IntegrationTestHost.BuildFactory(new WebApplicationFactory<TStartup>(), projectName),
            isThreadSafe: true);
    }

    public ValueTask DisposeIfCreated()
    {
        if (!Factory.IsValueCreated)
            return ValueTask.CompletedTask;

        object value = Factory.Value;

        if (value is IAsyncDisposable asyncDisposable)
            return asyncDisposable.DisposeAsync();

        if (value is IDisposable disposable)
            disposable.Dispose();

        return ValueTask.CompletedTask;
    }
}