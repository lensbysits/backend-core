using Lens.Core.Lib.Builders;
using Microsoft.Extensions.DependencyInjection;
using Lens.Services.Communication.Client;
using Lens.Services.Communication.Client.Services;

namespace Lens.Services.Communication.Tests;

public class CommunicationClientFixture : BaseFixture
{

    public ICommunicationService CommunicationService { get; private set; } = null!;

    protected override void OnSetupReady()
    {
        CommunicationService = Services.GetRequiredService<ICommunicationService>();
    }

    protected override void RegisterServices(IApplicationSetupBuilder applicationSetupBuilder)
    {
        applicationSetupBuilder
            .AddCommunicationClient();
    }
}
