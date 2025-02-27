using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Finite_State_Machine_Designer.Configuration.ExternalLogins
{
    public static class ExternalGoogle
    {
        public static void Setup(GoogleAuth externalAuth, AuthenticationBuilder authBuilder, string accessDeniedPath)
        {
            if (!string.IsNullOrWhiteSpace(externalAuth.ClientId)
                | !string.IsNullOrWhiteSpace(externalAuth.ClientSecret))
            {
                authBuilder.AddOpenIdConnect("Google", "Google", googleOptions =>
                {
                    googleOptions.CallbackPath = "/Account/ExternalLogin/Google";
                    googleOptions.SaveTokens = true;
                    googleOptions.AccessDeniedPath = accessDeniedPath;
                    googleOptions.MetadataAddress = externalAuth.MetaAuthUrl.ToString();
                    googleOptions.ClientId = externalAuth.ClientId;
                    googleOptions.ClientSecret = externalAuth.ClientSecret;
                    googleOptions.ResponseType = OpenIdConnectResponseType.Code;
                    googleOptions.Scope.Add("openid");
                    googleOptions.Scope.Add("email");
                    googleOptions.SkipUnrecognizedRequests = true; // Fixes the message.state empty error
                });
            }
        }
    }
}
