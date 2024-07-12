using Lens.Services.Communication.Models;

namespace Lens.Services.Communication.Client.Builders;

public class SmsMessageBuilder
{
    private readonly CommunicationMessageBuilder _communicationMessageBuilder;
    private readonly SendSmsBM _smsInfo = new();

    public SmsMessageBuilder(CommunicationMessageBuilder communicationMessageBuilder)
    {
        _communicationMessageBuilder = communicationMessageBuilder;
    }

    public SmsMessageBuilder ToNumber(string number)
    {
        _smsInfo.PhoneNumber = number;
        return this;
    }


    public SmsMessageBuilder UseText(string text)
    {
        _smsInfo.Text = text;
        return this;
    }

    public CommunicationMessageBuilder And()
    {
        return _communicationMessageBuilder.SetSmsInfo(_smsInfo);
    }
}
