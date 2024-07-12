using Lens.Services.Communication.Models;

namespace Lens.Services.Communication.Client.Builders
{
    public class EmailMessageBuilder
    {
        private readonly CommunicationMessageBuilder _communicationMessageBuilder;
        private readonly SendEmailBM _emailInfo = new();

        public EmailMessageBuilder(CommunicationMessageBuilder communicationMessageBuilder)
        {
            _communicationMessageBuilder = communicationMessageBuilder;
        }

        public EmailMessageBuilder From(EmailAddressBM from)
        {
            if (!string.IsNullOrEmpty(from))
                _emailInfo.From = from;
            return this;
        }

        /// <summary>
        /// can be semi-colon delimited.
        /// </summary>
        /// <param name="recipient"></param>
        /// <returns></returns>
        public EmailMessageBuilder To(string recipient)
        {
            if (!string.IsNullOrEmpty(recipient))
                _emailInfo.To = recipient;
            return this;
        }

        public EmailMessageBuilder Cc(string recipient)
        {
            if (!string.IsNullOrEmpty(recipient))
                _emailInfo.CC = recipient;
            return this;
        }

        public EmailMessageBuilder Bcc(string recipient)
        {
            if(!string.IsNullOrEmpty(recipient))
                _emailInfo.BCC = recipient;
            return this;
        }

        public EmailMessageBuilder Subject(string subject)
        {
            _emailInfo.Subject = subject;
            return this;
        }

        public EmailMessageBuilder WithTemplate(string template, TemplateTypeEnum ofType = TemplateTypeEnum.Plain)
        {
            _emailInfo.Template = new EmailTemplateBM { Template = template, TemplateType = ofType };
            return this;
        }

        public EmailMessageBuilder UsingData(dynamic data)
        {
            _emailInfo.Data = data;
            return this;
        }

        /// <summary>
        /// This call will push the email to the CommunicationMessageBuilder to actually be used.
        /// </summary>
        /// <returns>The CommunicationMessageBuilder responsible for sending this email.</returns>
        public CommunicationMessageBuilder And()
        {
            return _communicationMessageBuilder.SetEmailInfo(_emailInfo);
        }
    }
}
