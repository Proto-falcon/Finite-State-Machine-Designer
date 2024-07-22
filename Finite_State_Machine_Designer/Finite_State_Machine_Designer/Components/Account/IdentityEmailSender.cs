using Microsoft.AspNetCore.Identity;
using MailKit.Security;
using MailKit.Net.Smtp;
using Finite_State_Machine_Designer.Data;
using MimeKit;
using Microsoft.Extensions.Options;
using Finite_State_Machine_Designer.Configuration;

namespace Finite_State_Machine_Designer.Components.Account
{
    public class IdentityEmailSender
        : IEmailSender<ApplicationUser>
    {
        private readonly EmailServiceConfig _config;
        private readonly FormatOptions _formatOptions = new()
        {
            EnsureNewLine = true
        };
        private readonly SmtpClient _smtpClient;
        private readonly string _confirmEmailContent;
        private readonly ILogger<IdentityEmailSender> _logger;

        public IdentityEmailSender(IOptions<EmailServiceConfig> config,
            IOptions<EmailContentPaths> contentPaths,
            ILogger<IdentityEmailSender> logger)
        {
            _config = config.Value;
            _confirmEmailContent 
                = File.ReadAllText(contentPaths.Value.Confirmation);
            _logger = logger;
            _smtpClient = InitialiseClient()
                ?? throw new NullReferenceException(
                    "Couldn't initialise/connect smtp client to server");
        }

        public SmtpClient? InitialiseClient()
        {
            try
            {
                var emailClient = new SmtpClient();
                emailClient.Connect("smtp.gmail.com",
                    465, SecureSocketOptions.SslOnConnect);
                
                emailClient.Authenticate(
                    new SaslMechanismLogin(
                        _config.DisplayAddress, _config.Password)
                );
                return emailClient;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Couldn't initialise/connect smtp client to server");
                _logger.LogError("{Error}", ex.ToString());
                return null;
            }
        }

        public async Task SendConfirmationLinkAsync(ApplicationUser user,
            string email, string confirmationLink)
        {
            try
            {
                _ = MailboxAddress.TryParse(
                    _config.DisplayAddress, out MailboxAddress fromAddress);
                if (MailboxAddress.TryParse(
                    email, out MailboxAddress toAddress))
                {
                    var message = new MimeMessage();
                    message.From.Add(fromAddress);
                    message.To.Add(toAddress);
                    message.Subject = "FSM Designer Confirm Email";

                    message.Body = new TextPart("html")
                    {
                        Text = _confirmEmailContent
                            .Replace("{Confirmation Link}", confirmationLink)
                    };

                    string response
                        = await _smtpClient.SendAsync(_formatOptions, message);
                    _logger.LogInformation(
                        "'smtp.gmail.com' response message: {Response}", response);
                }
                else
                    throw new FormatException(
                        $"Invalid recipient email address - '{email}'");
            }
            catch (Exception ex)
            {
                _logger.LogError("{Error}", ex.ToString());
            }
        }

        public Task SendPasswordResetCodeAsync(ApplicationUser user,
            string email, string resetCode)
        {
            _logger.LogInformation(
                "Password reset email Address: {Address}", _config.DisplayAddress);
            return Task.CompletedTask;
        }

        public Task SendPasswordResetLinkAsync(ApplicationUser user,
            string email, string resetLink)
        {
            _logger.LogInformation(
                "Password reset email Address: {Address}", _config.DisplayAddress);
            return Task.CompletedTask;
        }

        ~IdentityEmailSender()
        {
            _smtpClient?.Disconnect(true);
            _smtpClient?.Dispose();
        }
    }
}
