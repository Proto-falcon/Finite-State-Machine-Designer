using Microsoft.AspNetCore.Authentication;

namespace Finite_State_Machine_Designer.Configuration.ExternalLogins
{
    public class ExternalDiscord
    {
        public static void Setup(DiscordAuth externalAuth, AuthenticationBuilder authBuilder, string accessDeniedPath)
        {
            if (!string.IsNullOrWhiteSpace(externalAuth.ClientId)
                | !string.IsNullOrWhiteSpace(externalAuth.ClientSecret))
            {
                authBuilder.AddDiscord(discordOptions =>
                {
                    discordOptions.CallbackPath = "/Account/ExternalLogin/Discord/";
                    discordOptions.SaveTokens = true;
                    discordOptions.AccessDeniedPath = accessDeniedPath;
                    discordOptions.ClientId = externalAuth.ClientId;
                    discordOptions.ClientSecret = externalAuth.ClientSecret;
                    discordOptions.Scope.Add("email");
                    discordOptions.Validate();
                });
            }
        }
    }
}
