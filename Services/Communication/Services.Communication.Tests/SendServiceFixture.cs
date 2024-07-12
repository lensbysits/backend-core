using Lens.Core.Lib.Builders;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Lens.Services.Communication.Tests;

public class SendServiceFixture : BaseFixture
{

    public ISenderService SenderService { get; private set; } = null!;

    protected override void OnSetupReady()
    {
        SenderService = Services.GetRequiredService<ISenderService>();
    }

    protected override void RegisterServices(IApplicationSetupBuilder applicationSetupBuilder)
    {
        applicationSetupBuilder.Services.AddControllersWithViews();
        var diagnosticListner = new DiagnosticListener("abc");
        applicationSetupBuilder.Services.AddSingleton(diagnosticListner);
        applicationSetupBuilder.Services.AddSingleton<DiagnosticSource>(diagnosticListner);

        applicationSetupBuilder
            .AddCommunicationServices();
    }
}
