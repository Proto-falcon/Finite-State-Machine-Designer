using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using Finite_State_Machine_Designer.Configuration;
using Finite_State_Machine_Designer.Services;
using Finite_State_Machine_Designer.Data.Identity;

namespace Finite_State_Machine_Designer.Components.Account
{
    public class IdentityEmailSender
        : IEmailSender<ApplicationUser>
    {
        private readonly SmtpFactory _smtpFactory;
        private readonly FormatOptions _formatOptions = new()
        {
            EnsureNewLine = true
        };
        private readonly MailboxAddress _fromServiceAddress;
        private readonly string _confirmEmailContentText;
        private readonly string _confirmEmailContentHtml;
        private readonly ILogger<IdentityEmailSender> _logger;

        public IdentityEmailSender(IOptions<EmailServiceConfig> config,
            SmtpFactory smtpFactory,
            IOptions<EmailContentPaths> contentPaths,
            ILogger<IdentityEmailSender> logger)
        {
            _smtpFactory = smtpFactory;
            string htmlContentPath = contentPaths.Value.EmailLayout;
            _confirmEmailContentHtml 
                = File.ReadAllText(htmlContentPath);
            _confirmEmailContentText
                = File.ReadAllText(htmlContentPath.Replace("html", "txt"));
            _logger = logger;
            if (!MailboxAddress.TryParse(
                config.Value.DisplayAddress, out _fromServiceAddress))
                throw new FormatException(
                    $"From address '{config.Value.DisplayAddress}' that successfully connected"
                    + " to email server had invalid email format");
        }

        public Task SendConfirmationLinkAsync(ApplicationUser user,
            string email, string confirmationLink)
            => SendMailAsync(email, "Confirm Email",
                $@"Click <a href=""{confirmationLink}"">here</a>"
                + @" to confirm your email address.",
                $"Couldn't send the confirmation link to {email}"
                + " due to SMTP client unablet to connect at the moment.");

        public Task SendPasswordResetCodeAsync(ApplicationUser user,
            string email, string resetCode)
            => SendMailAsync(email, "Reset Password Code",
                $@"Enter this code <b>{resetCode}</b>"
                + @" to reset your password.",
                $"Couldn't send the reset password code to {email}"
                + " due to SMTP client unablet to connect at the moment.");

        public Task SendPasswordResetLinkAsync(ApplicationUser user,
            string email, string resetLink)
            => SendMailAsync(email, "Reset Password",
                $@"Click <a href=""{resetLink}"">here</a>"
                + @" to reset your password.",
                $"Couldn't send the reset password link to {email}"
                + " due to SMTP client unablet to connect at the moment.");

        private async Task SendMailAsync(string email, string title,
            string message, string? errorMessage = null)
        {
            try
            {
                if (await _smtpFactory.InitialiseClient() is SmtpClient smtpClient
                    && MailboxAddress.TryParse(email, out MailboxAddress toAddress))
                {
                    var mailMessage = new MimeMessage();
                    mailMessage.From.Add(_fromServiceAddress);
                    mailMessage.To.Add(toAddress);
                    mailMessage.Subject = $"FSM Designer - {title}";

                    mailMessage.Body = new BodyBuilder()
                    {
                        TextBody = _confirmEmailContentText
                            .Replace("{Title}", title)
                            .Replace("{Message}", message),
                        HtmlBody = _confirmEmailContentHtml
                            .Replace("{Title}", title)
                            .Replace("{Message}", message)
                    }.ToMessageBody();

                    string response
                        = await smtpClient.SendAsync(_formatOptions, mailMessage);
                    _logger.LogInformation(
                        "SMTP server response message: {Response}", response);

                    await smtpClient.DisconnectAsync(true);
                    smtpClient.Dispose();
                }
                else
                    throw new InvalidOperationException(
                        string.IsNullOrWhiteSpace(errorMessage) ?
                        $"Couldn't send email to {email}." : errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Error}", ex.ToString());
                throw;
            }
        }
    }
}
