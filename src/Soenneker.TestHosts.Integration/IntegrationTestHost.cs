using Bogus;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Soenneker.Enums.DeployEnvironment;
using Soenneker.Serilog.Sinks.TUnit;
using Soenneker.StartupFilters.IntegrationTests.Registrars;
using Soenneker.TestHosts.Integration.Abstract;
using Soenneker.Utils.AutoBogus;
using Soenneker.Utils.AutoBogus.Config;
using Soenneker.Utils.Jwt.Registrars;
using Soenneker.Utils.Test.AuthHandler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Soenneker.Extensions.ValueTask;

namespace Soenneker.TestHosts.Integration;

///<inheritdoc cref="IIntegrationTestHost"/>
public class IntegrationTestHost : IIntegrationTestHost
{
    private static readonly ConcurrentDictionary<string, string> _appSettingsPathCache = new(StringComparer.Ordinal);

    private readonly Dictionary<Type, IFactoryHolder> _factories = new();

    public Faker Faker { get; private set; }
    public AutoFaker AutoFaker { get; private set; }
    public AutoFakerConfig? AutoFakerConfig { get; set; }

    public virtual Task InitializeAsync()
    {
        AutoFakerConfig config = AutoFakerConfig ?? new AutoFakerConfig();
        AutoFaker = new AutoFaker(config);
        Faker = AutoFaker.Faker;
        return Task.CompletedTask;
    }

    public void RegisterFactory<TStartup>(string projectName) where TStartup : class
    {
        Type type = typeof(TStartup);

        if (_factories.ContainsKey(type))
            return;

        _factories[type] = new FactoryHolder<TStartup>(projectName);
    }

    public Lazy<WebApplicationFactory<TStartup>> GetFactory<TStartup>() where TStartup : class
    {
        if (_factories.TryGetValue(typeof(TStartup), out IFactoryHolder? holder))
            return ((FactoryHolder<TStartup>)holder).Factory;

        throw new InvalidOperationException($"Factory for type {typeof(TStartup).Name} has not been registered.");
    }

    internal static WebApplicationFactory<T> BuildFactory<T>(WebApplicationFactory<T> factory, string projectName) where T : class
    {
        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, configBuilder) =>
            {
                string appSettingsPath = GetAppSettingsPath(projectName);
                configBuilder.AddJsonFile(appSettingsPath, optional: false, reloadOnChange: false);
            });

            builder.ConfigureTestServices(static services =>
            {
                services.AddJwtUtilAsScoped();
                services.AddIntegrationTestsStartupFilterAsSingleton();

                services.AddAuthentication(DeployEnvironment.Test.Name).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(DeployEnvironment.Test.Name,
                    static _ =>
                    {
                    });
            });

            builder.ConfigureServices(static services =>
            {
                services.AddSingleton<TUnitTestContextSink>();

                services.AddSerilog((sp, loggerConfiguration) =>
                {
                    var sink = sp.GetRequiredService<TUnitTestContextSink>();

                    loggerConfiguration.MinimumLevel.Verbose().WriteTo.Async(a => a.Sink(sink)).Enrich.FromLogContext();
                });
            });
        });
    }

    public static string GetAppSettingsPath(string projectName)
    {
        return _appSettingsPathCache.GetOrAdd(projectName, static pn =>
        {
            string baseDir = AppContext.BaseDirectory;

            string? parent = Directory.GetParent(baseDir)?.FullName;

            if (string.IsNullOrWhiteSpace(parent))
                throw new Exception($"AppSettings path does not exist! baseDir: {baseDir}");

            string path = Path.Combine(parent, pn, "appsettings.json");

            if (!File.Exists(path))
                throw new Exception($"appsettings.json file does not exist at {path}! baseDir: {baseDir}");

            return path;
        });
    }

    public async ValueTask DisposeAsync()
    {
        foreach (IFactoryHolder holder in _factories.Values)
        {
            await holder.DisposeIfCreated().NoSync();
        }
    }
}