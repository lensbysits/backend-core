using Lens.Services.Communication.Models;
using Lens.Services.Communication.Client.Services;

namespace Lens.Services.Communication.Client.Builders;

public class CommunicationMessageBuilder
{
    private readonly ICommunicationService _communicationService;
    private readonly SendBM _sendInfo = new();

    public CommunicationMessageBuilder(ICommunicationService communicationService)
    {
        _communicationService = communicationService;

    }

    public CommunicationMessageBuilder SetSmsInfo(SendSmsBM smsInfo)
    {
        _sendInfo.Sms = smsInfo;
        return this;
    }

    public CommunicationMessageBuilder SetEmailInfo(SendEmailBM emailInfo)
    {
        _sendInfo.Email = emailInfo;
        return this;
    }

    public SmsMessageBuilder CreateSmsMessage()
    {
        return new SmsMessageBuilder(this);
    }


    public EmailMessageBuilder CreateEmailMessage()
    {
        return new EmailMessageBuilder(this);
    }

    public async Task Send()
    {
        await _communicationService.Send(_sendInfo);
    }
}
