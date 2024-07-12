using Lamar;
using Lamar.Scanning.Conventions;
using Lens.Core.App;
using Lens.Core.Lib.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lens.Core.Tests;

public class BaseFixture : IDisposable
{
    public Container Services { get; private set; }

    public BaseFixture()
    {
        Services = new Container(registry =>
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.Development.json", true)
                .AddUserSecrets(GetType().Assembly);

            OnBuildConfiguration(configurationBuilder);

            var configuration = configurationBuilder.Build();

            registry
                .AddLogging();

            registry.Scan(scanner =>
            {
                scanner.WithDefaultConventions();
                scanner.AssemblyContainingType<BaseFixture>();
                OnScanningServices(scanner);
            });


            RegisterServices(new ApplicationSetupBuilder(registry, configuration).AddApplicationServices());
        });

        OnSetupReady();
    }

    public void Dispose()
    {
        Services.Dispose();
        OnDispose();
    }

    protected virtual void OnBuildConfiguration(IConfigurationBuilder configurationBuilder) { }
    protected virtual void OnScanningServices(IAssemblyScanner scanner) { }
    protected virtual void RegisterServices(IApplicationSetupBuilder applicationSetupBuilder) { }
    protected virtual void OnSetupReady() { }
    protected virtual void OnDispose() { }
}