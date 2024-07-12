using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Lens.Core.Lib.Builders;

public class ApplicationSetupBuilder : IApplicationSetupBuilder
{
    public IServiceCollection Services { get; }
    public IConfiguration Configuration { get; }


    public List<Assembly> Assemblies { get; } = new List<Assembly>();

    public ApplicationSetupBuilder(IServiceCollection services, IConfiguration configuration)
    {
        Services = services;
        Configuration = configuration;
        Assemblies.Add(Assembly.GetEntryAssembly()!);
    }

    /// <summary>
    /// Add assemblies that will be scanned for AutoMapper profiles and MediatR handlers.
    /// The entry-assembly is added by default.
    /// </summary>
    public IApplicationSetupBuilder AddAssemblies(params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            if (!Assemblies.Contains(assembly))
            {
                Assemblies.Add(assembly);
            }
        }

        return this;
    }

    public IApplicationSetupBuilder AddAutoMapper()
    {
        Services.AddAutoMapper(Assemblies);
        return this;
    }

    public IApplicationSetupBuilder AddMediatR()
    {
        Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assemblies.ToArray()));
        return this;
    }
}
