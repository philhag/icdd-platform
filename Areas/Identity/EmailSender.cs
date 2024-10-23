using System.Threading.Tasks;
using IcddWebApp.WebApplication.Environment;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace IcddWebApp.Areas.Identity
{
    namespace WebPWrecover.Services
    {
        public class EmailSender : IEmailSender
        {
            public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
            {
                Options = optionsAccessor.Value;
            }

            public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

            public Task SendEmailAsync(string email, string subject, string message)
            {
                return Execute(Options.SendGridKey, subject, message, email);
            }

            public Task Execute(string apiKey, string subject, string message, string email)
            {
                var client = new SendGridClient("{SendGridKeyInput}");
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress("icdd-plattform@ruhr-uni-bochum.de", "RUB ICDD Plattform"),
                    Subject = "[RUB ICDD Platform] "+ subject,
                    PlainTextContent = message,
                    HtmlContent = message
                };
                msg.AddTo(new EmailAddress(email));

                // Disable click tracking.
                // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
                msg.SetClickTracking(false, false);
                Logger.Log("Email gesendet: An:" + email + "<br> Text: " + message, Logger.MsgType.Info, "SendEmailAsync");
                return client.SendEmailAsync(msg);
            }
        }
    }
}
