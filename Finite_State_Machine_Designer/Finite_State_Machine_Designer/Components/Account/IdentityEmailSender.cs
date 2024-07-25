using Microsoft.AspNetCore.Identity;
using MailKit.Net.Smtp;
using Finite_State_Machine_Designer.Data;
using MimeKit;
using Microsoft.Extensions.Options;
using Finite_State_Machine_Designer.Configuration;
using Finite_State_Machine_Designer.Services;

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
            string htmlContentPath = contentPaths.Value.Confirmation;
            _confirmEmailContentHtml 
                = File.ReadAllText(htmlContentPath);
            _confirmEmailContentText
                = File.ReadAllText(htmlContentPath.Replace("html", "txt"));
            _logger = logger;
            if (!MailboxAddress.TryParse(
                config.Value.DisplayAddress, out _fromServiceAddress))
                throw new FormatException(
                    "From address '{FROM}' that successfully connected"
                    + " to smtp.gmail.com had invalid email format");
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user,
            string email, string confirmationLink)
        {
            try
            {
                if (await _smtpFactory.InitialiseClient() is SmtpClient smtpClient
                    && MailboxAddress.TryParse(email, out MailboxAddress toAddress))
                {
                    var message = new MimeMessage();
                    message.From.Add(_fromServiceAddress);
                    message.To.Add(toAddress);
                    message.Subject = "FSM Designer Confirm Email";

                    message.Body = new BodyBuilder()
                    {
                        TextBody = _confirmEmailContentText
                            .Replace("{Confirmation Link}", confirmationLink),
                        HtmlBody = _confirmEmailContentHtml
                            .Replace("{Confirmation Link}", confirmationLink)
                    }.ToMessageBody();

                    string response
                        = await smtpClient.SendAsync(_formatOptions, message);
                    _logger.LogInformation(
                        "'smtp.gmail.com' response message: {Response}", response);

                    await smtpClient.DisconnectAsync(true);
                    smtpClient.Dispose();
                }
                else
                {
                    _logger.LogError("Couldn't send {MsgPart2} {MsgPart3}",
                        "the confirmation link via email due to ",
                        "smtp client unavailable at the moment");
                    throw new InvalidOperationException(
                        $"Couldn't send Confirmation Email to address '{email}'"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("{Error}", ex.ToString());
                throw;
            }
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user,
            string email, string resetCode)
        {
            _logger.LogInformation(
                "Password reset email Address: {Address}", _fromServiceAddress);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user,
            string email, string resetLink)
        {
            _logger.LogInformation(
                "Password reset email Address: {Address}", _fromServiceAddress);
            return Task.CompletedTask;
        }
    }
}
