using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Finite_State_Machine_Designer.Configuration.ExternalLogins
{
    public static class ExternalSlack
    {
        public static void Setup(SlackAuth externalAuth, AuthenticationBuilder authBuilder, string accessDeniedPath)
        {
            if (!string.IsNullOrWhiteSpace(externalAuth.ClientId)
                | !string.IsNullOrWhiteSpace(externalAuth.ClientSecret))
            {
                /// 'Slack' authentication scheme is just a name I've assinged for this authentication
                /// use different shchemes for many others.
                authBuilder.AddOpenIdConnect("Slack", "Slack", slackOptions =>
                {
                    slackOptions.CallbackPath = "/Account/ExternalLogin/Slack";
                    slackOptions.SaveTokens = true;
                    slackOptions.AccessDeniedPath = accessDeniedPath;
                    slackOptions.MetadataAddress = externalAuth.MetaAuthUrl.ToString();
                    slackOptions.ClientId = externalAuth.ClientId;
                    slackOptions.ClientSecret = externalAuth.ClientSecret;
                    slackOptions.ResponseType = OpenIdConnectResponseType.Code;
                    slackOptions.Scope.Add("openid");
                    slackOptions.Scope.Add("email");
                    slackOptions.SkipUnrecognizedRequests = true; // Fixes the message.state empty error
                });
            }
        }
    }
}
