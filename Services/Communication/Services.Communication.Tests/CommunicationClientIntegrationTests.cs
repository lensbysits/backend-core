using Lens.Services.Communication.Models;

namespace Lens.Services.Communication.Tests;

public class CommunicationClientIntegrationTests : IClassFixture<CommunicationClientFixture>
{
    private readonly CommunicationClientFixture _fixture;

    public CommunicationClientIntegrationTests(CommunicationClientFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SendEmail()
    {
        var sendInfo = new SendBM
        {
            Email = new SendEmailBM
            {
                To = "johan@storteboom.net",
                Subject = "test email",
                Template = new EmailTemplateBM
                {
                    TemplateType = TemplateTypeEnum.Plain,
                    Template = "Message to communicate"
                }
            }
        };

        try
        {
            await _fixture.CommunicationService.Send(sendInfo);
                
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }

    [Fact]
    public async Task SendSms()
    {
        var sendInfo = new SendBM
        {
            Sms = new SendSmsBM
            {
                PhoneNumber = "0650841143",
                Text = "Message to communicate 2"
            }
        };

        try
        {
            await _fixture.CommunicationService.Send(sendInfo);
        }
        catch (Exception e)
        {
            Assert.Fail(e.Message);
        }
    }
}