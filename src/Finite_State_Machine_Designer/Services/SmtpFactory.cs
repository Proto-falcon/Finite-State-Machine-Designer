using Finite_State_Machine_Designer.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;

namespace Finite_State_Machine_Designer.Services
{
    public class SmtpFactory(IOptions<EmailServiceConfig> options,
        ILogger<SmtpFactory> logger)
    {
        private readonly EmailServiceConfig _config = options.Value;

        public async Task<SmtpClient?> InitialiseClient()
        {
            try
            {
                var emailClient = new SmtpClient();
                await emailClient.ConnectAsync(_config.SmtpServer,
                    _config.Port, SecureSocketOptions.StartTls);

                await emailClient.AuthenticateAsync(
                new SaslMechanismLogin(
                        _config.UserName, _config.Password)
                );
                return emailClient;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    "Couldn't initialise/connect smtp client to server");
                logger.LogError("{Error}", ex.ToString());
                return null;
            }
        }
    }
}
