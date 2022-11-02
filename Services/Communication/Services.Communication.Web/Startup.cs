﻿using Lens.Core.App.Web.Builders;
using Lens.Services.Communication.Models;

namespace Lens.Services.Communication.Web;

public class Startup : Core.App.Web.StartupBase
{
    public Startup(IConfiguration configuration) : base(configuration)
    {
    }

    public override void OnSetupApplication(IWebApplicationSetupBuilder applicationSetup)
    {
        applicationSetup
            // Add app specific services.
            .AddCommunicationServices();
    }
}
