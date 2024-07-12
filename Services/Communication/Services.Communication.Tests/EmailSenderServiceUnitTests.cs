using Lens.Core.Lib.Services;
using Lens.Services.Communication.Settings;

namespace Lens.Services.Communication.Tests;

public class EmailSenderServiceUnitTests
{
    [Theory]
    [InlineData("johan@storteboom.net", "johan@storteboom.net")]
    [InlineData("johan+550@storteboom.net", "johan@storteboom.net")]
    public void SanitizeEmail(string emailIn, string emailOut)
    {
        var appServiceMock = new Mock<IApplicationService<EmailSenderService, SendEmailSettings>>();
        appServiceMock.SetupGet(a => a.Settings).Returns(new SendEmailSettings { SenderAddress = "test@test.com"});
        var sender = new EmailSenderService(appServiceMock.Object, Mock.Of<ITemplateRenderServiceFactory>());

        var result = sender.SanatizeEmail(emailIn);
        Assert.Equal(emailOut, result);
    }
}
