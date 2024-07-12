using Lens.Core.Lib.Services;
using Lens.Services.Communication.Models;
using Microsoft.Extensions.Logging;

namespace Lens.Services.Communication;

public class SenderService : BaseService<SenderService>, ISenderService
{
    private readonly IEmailSenderService _emailSender;
    private readonly ISmsSenderService _smsSender;

    public SenderService(
        IApplicationService<SenderService> applicationService,
        IEmailSenderService emailSender,
        ISmsSenderService smsSender) : base(applicationService)
    {
        _emailSender = emailSender;
        _smsSender = smsSender;
    }

    public async Task Send(SendBM sendInfo)
    {
        if(sendInfo.Email != null)
        {
            try
            {
                sendInfo.Email.SetTypedModel<dynamic>();
                var message = await _emailSender.Send<dynamic>(
                    sendInfo.Email.Template, 
                    sendInfo.Email.Data, 
                    sendInfo.Email.To, 
                    sendInfo.Email.CC, 
                    sendInfo.Email.BCC, 
                    sendInfo.Email.Subject,
                    sendInfo.Email.From);
            }
            catch (Exception e)
            {
                ApplicationService.Logger.LogError(e, "An error occured when try to send an email using info {sendInfo}", sendInfo.Email);
            }   
        }

        if (sendInfo.Sms != null)
        {
            try
            {
                await _smsSender.Send(sendInfo.Sms);
            }
            catch (Exception e)
            {
                ApplicationService.Logger.LogError(e, "An error occured when try to send an sms using info {sendInfo}", sendInfo.Sms);
            }
        }
    }
}
