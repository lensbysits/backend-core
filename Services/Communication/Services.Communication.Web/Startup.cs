using Lens.Core.App.Web.Builders;

namespace Lens.Services.Communication.Web;

public class Startup : Core.App.Web.StartupBase
{
    public Startup(IConfiguration configuration) : base(configuration)
    {
    }

    public override void OnSetupApplication(IWebApplicationSetupBuilder applicationSetup)
    {
        applicationSetup.Controller
            .UseViews()
            .JsonSerializeEnumsAsStrings();

        applicationSetup
            // Add app specific services.
            .AddCommunicationServices();
    }
}
