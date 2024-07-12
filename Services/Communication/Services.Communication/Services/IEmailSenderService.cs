using Lens.Services.Communication.Models;
using Lens.Services.Communication.Settings;
using MimeKit;

namespace Lens.Services.Communication;

public interface IEmailSenderService
{
    Task<MimeMessage?> Send<TModel>(EmailTemplateBM template, TModel data, string toAddress, string ccAddress, string bccAddress, string subject, EmailAddressBM? fromAdress = null, IEnumerable<EmailAttachmentBM>? attachments = null);
    Task<MimeMessage?> Send<TModel>(EmailTemplateBM template, TModel data, string toAddress, string ccAddress, string bccAddress, string emailName, string subject, EmailAddressBM? fromAdress = null, IEnumerable<EmailAttachmentBM>? attachments = null);
    Task<MimeMessage?> Send<TModel>(EmailTemplateBM template, TModel data, IEnumerable<EmailAddressBM> toAddresses, IEnumerable<EmailAddressBM> ccAddresses, IEnumerable<EmailAddressBM> bccAddresses, string subject, EmailAddressBM? fromAdress = null, IEnumerable<EmailAttachmentBM>? attachments = null);
    SendEmailSettings Settings { get; }
}