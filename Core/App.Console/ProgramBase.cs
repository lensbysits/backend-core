using Lens.Core.Lib.Builders;
using Microsoft.Extensions.Hosting;

namespace Lens.Core.App.Console;

public abstract class ProgramBase : App.ProgramBase
{
    protected static Action<HostBuilderContext, IApplicationSetupBuilder>? ApplicationSetup { get; set; }

    public static Task<int> Start(string[] args) => Start(args, CreateHostBuilder);

    protected static IHostBuilder CreateHostBuilder(string[] args) =>
            CreateBaseHostBuilder(args)
                .UseConsoleLifetime()
                .ConfigureServices((context, services) =>
                {
                    var applicationSetup = new ApplicationSetupBuilder (services, context.Configuration);
                    ApplicationSetup?.Invoke(context, applicationSetup);
                    applicationSetup.AddApplicationServices();
                    applicationSetup.AddAssemblySpecificApplicationServices();
                });
}
