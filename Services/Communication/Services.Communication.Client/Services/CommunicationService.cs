using Lens.Core.Lib.Services;
using Lens.Services.Communication.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Lens.Services.Communication.Client.Services
{
    public class CommunicationService : BaseService<CommunicationService>, ICommunicationService
    {
        private readonly HttpClient _httpClient;

        public CommunicationService(
            IApplicationService<CommunicationService> applicationService,
            HttpClient httpClient) : base(applicationService)
        {
            _httpClient = httpClient;
        }

        public async Task Send(SendBM sendInfo)
        {
            if (sendInfo?.Email == null && sendInfo?.Sms == null)
            {
                throw new ArgumentException("Either Email or Sms info must be supplier.", nameof(sendInfo));
            }

            try
            {
                var result = await _httpClient.PostAsync("send", new StringContent(JsonSerializer.Serialize(sendInfo), System.Text.Encoding.UTF8, "application/json"));
                if (!result.IsSuccessStatusCode)
                {
                    var responseText = await result.Content.ReadAsStringAsync();
                    ApplicationService.Logger.LogWarning("Sending a message failed with response: {responseText}.", responseText);
                }
            }
            catch (Exception e)
            {
                ApplicationService.Logger.LogError(e, "An error occured while sending a message.");
            }
        }
    }
}
