using Microsoft.AspNetCore.Authentication;

namespace Finite_State_Machine_Designer.Configuration.ExternalLogins
{
    public class ExternalGithub
    {
        public static void Setup(GithubAuth externalAuth, AuthenticationBuilder authBuilder, string accessDeniedPath)
        {
            if (!string.IsNullOrWhiteSpace(externalAuth.ClientId)
                | !string.IsNullOrWhiteSpace(externalAuth.ClientSecret))
            {
                authBuilder.AddGitHub(githubOptions =>
                {
                    githubOptions.CallbackPath = "/Account/ExternalLogin/Github/";
                    githubOptions.SaveTokens = true;
                    githubOptions.AccessDeniedPath = accessDeniedPath;
                    githubOptions.ClientId = externalAuth.ClientId;
                    githubOptions.ClientSecret = externalAuth.ClientSecret;
                    githubOptions.Scope.Add("user:email");
                    githubOptions.Validate();
                });
            }
        }
    }
}
