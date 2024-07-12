using Lens.Services.Communication.Models;
using Lens.Services.Communication.Tests;

namespace Services.Communication.Tests
{
    public class EmailIntegrationTests : IClassFixture<SendServiceFixture>
    {
        private readonly SendServiceFixture _fixture;

        public EmailIntegrationTests(SendServiceFixture fixture)
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
                await _fixture.SenderService.Send(sendInfo);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }
    }
}