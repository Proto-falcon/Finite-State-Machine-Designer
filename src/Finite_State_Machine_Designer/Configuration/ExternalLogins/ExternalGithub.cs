using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Finite_State_Machine_Designer.Configuration.ExternalLogins.Github;

namespace Finite_State_Machine_Designer.Configuration.ExternalLogins
{
    public class ExternalGithub
    {
        public static void Setup(GithubAuth externalAuth, AuthenticationBuilder authBuilder, string accessDeniedPath)
        {
            if (!string.IsNullOrWhiteSpace(externalAuth.ClientId)
                | !string.IsNullOrWhiteSpace(externalAuth.ClientSecret))
            {
                authBuilder.AddOAuth<GitHubAuthenticationOptions,GitAuthHandler>(
                    GitHubAuthenticationDefaults.AuthenticationScheme, GitHubAuthenticationDefaults.DisplayName, githubOptions =>
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
