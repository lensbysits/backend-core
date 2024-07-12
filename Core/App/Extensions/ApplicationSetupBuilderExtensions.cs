using Lens.Core.App.Services;
using Lens.Core.Lib;
using Lens.Core.Lib.Builders;
using Lens.Core.Lib.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lens.Core.App;

public static class ApplicationSetupBuilderExtensions
{
    public static IApplicationSetupBuilder AddApplicationServices(this IApplicationSetupBuilder applicationSetup)
    {
        applicationSetup
            .AddBackgroundTaskQueue()
            .Services
                .AddScoped(typeof(IApplicationService<>), typeof(ApplicationService<>))
                .AddScoped(typeof(IApplicationService<,>), typeof(ApplicationService<,>));

        return applicationSetup;
    }

    /// <summary>
    /// These services are dependent on the assemblies you add to the services for scanning, so should run AFTER the service collection has been built.
    /// </summary>
    /// <param name="applicationSetup"></param>
    /// <returns></returns>
    public static IApplicationSetupBuilder AddAssemblySpecificApplicationServices(this IApplicationSetupBuilder applicationSetup)
    {
        applicationSetup
            .AddAutoMapper()
            .AddMediatR();

        return applicationSetup;
    }
}