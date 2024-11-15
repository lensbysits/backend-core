using Lens.Core.Lib;
using Lens.Core.Lib.Builders;
using Lens.Services.Communication.Client.Services;
using Lens.Services.Communication.Client.Settings;
using Microsoft.Extensions.Configuration;

namespace Lens.Services.Communication.Client;

public static class IApplicationSetupBuilderExtentions
{
    private const string oauthClientConfigurationKey = "communication";

    public static IApplicationSetupBuilder AddCommunicationClient(this IApplicationSetupBuilder builder)
    {
        var settings = builder.Configuration.GetSection(nameof(CommunicationSettings))?.Get<CommunicationSettings>() ?? throw new Exception($"Missing settings for '{nameof(CommunicationSettings)}'");
        builder
            .AddOAuthClientServices()
            .AddHttpClientService<ICommunicationService, CommunicationService>(oauthClientConfigurationKey, settings.Uri);

        return builder;
    }
}
