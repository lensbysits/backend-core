using Lens.Core.App.Web.Options;
using Lens.Core.Lib.Builders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lens.Core.App.Web.Builders;

internal class WebApplicationSetupBuilder : ApplicationSetupBuilder, IWebApplicationSetupBuilder
{
    public IHealthChecksBuilder HealthChecks { get; }

    public IControllerOptions Controller { get => ControllerOptions; }
    internal ControllerOptions ControllerOptions { get; } = new ControllerOptions();

    public IAuthOptions Auth { get => AuthOptions; }
    internal AuthOptions AuthOptions { get; } = new AuthOptions();

    public WebApplicationSetupBuilder(IServiceCollection services, IConfiguration configuration) : base(services, configuration)
    {
        HealthChecks = services.AddHealthChecks();
    }
}
