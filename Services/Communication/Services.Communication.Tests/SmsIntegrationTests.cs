using Lens.Services.Communication.Models;
using Lens.Services.Communication.Tests;

namespace Services.Communication.Tests
{
    public class SmsIntegrationTests : IClassFixture<SendServiceFixture>
    {
        private readonly SendServiceFixture _fixture;

        public SmsIntegrationTests(SendServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SendSms()
        {
            var sendInfo = new SendBM
            {
                Sms = new SendSmsBM
                {
                    PhoneNumber = "0650841143",
                    Text = "Message to communicate"
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