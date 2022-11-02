﻿using Lens.Core.Lib.Services;
using Lens.Services.Communication.Models;
using Lens.Services.Communication.Settings;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Lens.Services.Communication;

public class EmailSenderService : BaseService<EmailSenderService, SendEmailSettings>, IEmailSenderService
{
    private ITemplateRenderServiceFactory _templateRenderServiceFactory;

    public SendEmailSettings Settings => ApplicationService.Settings;

    public EmailSenderService(
        IApplicationService<EmailSenderService, SendEmailSettings> applicationService,
        ITemplateRenderServiceFactory templateRenderServiceFactory) : base(applicationService)
    {
        _templateRenderServiceFactory = templateRenderServiceFactory;
    }

    public Task<MimeMessage?> Send<TModel>(EmailTemplateBM template, TModel data, string toAddress, string ccAddress, string bccAddress, string subject, IEnumerable<EmailAttachmentBM>? attachments = null)
    {
        return Send(
            template, 
            data, 
            new[] { new EmailAddressBM { Email = toAddress } },
            new[] { new EmailAddressBM { Email = ccAddress } },
            new[] { new EmailAddressBM { Email = bccAddress } },
            subject, 
            attachments);
    }

    public Task<MimeMessage?> Send<TModel>(EmailTemplateBM template, TModel data, string toAddress, string ccAddress, string bccAddress, string emailName, string subject, IEnumerable<EmailAttachmentBM>? attachments = null)
    {
        return Send(
            template, 
            data, 
            new[] { new EmailAddressBM { Email = toAddress, Name = emailName } },
            new[] { new EmailAddressBM { Email = ccAddress } },
            new[] { new EmailAddressBM { Email = bccAddress } },
            subject, 
            attachments);
    }

    public async Task<MimeMessage?> Send<TModel>(EmailTemplateBM template, TModel data, IEnumerable<EmailAddressBM> toAddresses, IEnumerable<EmailAddressBM> ccAddresses, IEnumerable<EmailAddressBM> bccAddresses, string subject, IEnumerable<EmailAttachmentBM>? attachments = null)
    {
        if (!toAddresses?.Any() ?? true)
        {
            ApplicationService.Logger.LogError($"Attemt to send email using template {template} failed. No emailAddresses were given.");
            throw new Exception("No send-to emailaddresses provided");
        }

        ApplicationService.Logger.LogInformation($"About to send an email to {toAddresses!.First().Name} - {toAddresses!.First().Email}");

        string body = string.Empty;
        try
        {
            var renderer = _templateRenderServiceFactory.GetTemplateRenderService(template.TemplateType);
            body = await renderer.RenderToStringAsync(template.Template ?? String.Empty, data);
        }
        catch (Exception e)
        {
            ApplicationService.Logger.LogError("An error has occured when rendering the template", e);
            throw;
        }

        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress(ApplicationService.Settings.SenderName, ApplicationService.Settings.SenderAddress));

        // Use a fixed email-address when one is provided in the configuration (for dev-purposes)
        var onlySendTo = !string.IsNullOrEmpty(ApplicationService.Settings.OnlySendTo);
        if (onlySendTo)
        {
            SetAddressField(new[] { new EmailAddressBM { Email = ApplicationService.Settings.OnlySendTo } }, emailMessage.To);
        }
        else
        {
            SetAddressField(toAddresses!, emailMessage.To);
            SetAddressField(ccAddresses, emailMessage.Cc);

            var bccAddressList = (bccAddresses ?? new List<EmailAddressBM>()).ToList();
            if (!string.IsNullOrEmpty(ApplicationService.Settings.AlwaysBccTo))
                bccAddressList.Add(new EmailAddressBM { Email = ApplicationService.Settings.AlwaysBccTo });
            SetAddressField(bccAddressList, emailMessage.Bcc);
        }

        // If a prefix is in the configuration, add it to the subject (for dev-purposes).
        if (!string.IsNullOrEmpty(ApplicationService.Settings.SubjectPrefix))
            subject = ApplicationService.Settings.SubjectPrefix + subject;
        emailMessage.Subject = subject;

        // build body with attachments
        var bodyBuilder = new BodyBuilder();
        if (attachments != null && attachments.Any())
        {
            foreach (var attachment in attachments)
            {
                bodyBuilder.Attachments.Add(attachment.FileName, attachment.Data);
            }
        }
        bodyBuilder.HtmlBody = body;
        emailMessage.Body = bodyBuilder.ToMessageBody();

        // If the config says not to actually send, then lets get out of here (for dev-purposes).
        if (!ApplicationService.Settings.ActuallySendEmails)
        {
            ApplicationService.Logger.LogInformation($"Setting ActuallySendEmails is off. E-mail is not send");
            return emailMessage;
        }

        using (var client = new SmtpClient())
        {
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            try
            {
                await client.ConnectAsync(ApplicationService.Settings.Host, ApplicationService.Settings.Port, ApplicationService.Settings.NoSecurity ? MailKit.Security.SecureSocketOptions.None : MailKit.Security.SecureSocketOptions.Auto);
                if (!ApplicationService.Settings.NoAuthentication)
                {
                    await client.AuthenticateAsync(ApplicationService.Settings.Username, ApplicationService.Settings.Password);
                }
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                return emailMessage;
            }
            catch (Exception e)
            {
                ApplicationService.Logger.LogError("An error has occured when trying to send email\n{error}", e.Message);
            }
        }

        return null;
    }

    private void SetAddressField(IEnumerable<EmailAddressBM> addresses, InternetAddressList emailMessageAddresses)
    {
        if (addresses?.Any() ?? false)
        {
            addresses
                .ToList()
                .ForEach(emailAddress =>
                {
                    emailAddress.Email?
                        .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList()
                        .ForEach(ea =>
                        {
                            string toEmail = SanatizeEmail(ea);
                            emailMessageAddresses.Add(new MailboxAddress(emailAddress.Name ?? "", toEmail));
                        });
                });
        }
    }

    private string SanatizeEmail(string email)
    {
        var plusIndex = email.IndexOf('+');
        if (plusIndex > 0)
        {
            var atIndex = email.IndexOf('@');
            email = email[..plusIndex] + email[atIndex..];
        }

        return email;
    }

}
