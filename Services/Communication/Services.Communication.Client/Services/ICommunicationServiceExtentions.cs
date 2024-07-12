using Lens.Services.Communication.Client.Builders;

namespace Lens.Services.Communication.Client.Services;

public static class ICommunicationServiceExtentions
{
    public static CommunicationMessageBuilder CreateMessage(this ICommunicationService communicationService)
    {
        return new CommunicationMessageBuilder(communicationService);
    }
}
