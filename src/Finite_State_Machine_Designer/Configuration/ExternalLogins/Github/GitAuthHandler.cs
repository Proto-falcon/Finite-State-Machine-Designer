using System.Text.Json;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Diagnostics.CodeAnalysis;
using AspNet.Security.OAuth.GitHub;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Finite_State_Machine_Designer.Configuration.ExternalLogins.Github
{
    public class GitAuthHandler(IOptionsMonitor<GitHubAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : GitHubAuthenticationHandler(options, logger, encoder)
    {
        protected override async Task<AuthenticationTicket> CreateTicketAsync(
        [NotNull] ClaimsIdentity identity,
        [NotNull] AuthenticationProperties properties,
        [NotNull] OAuthTokenResponse tokens)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            using var response = await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("An error occurred while retrieving the user profile: the remote server returned a {Status} response with the following payload: {Headers} {Body}.",
                    response.StatusCode, response.Headers, await response.Content.ReadAsStringAsync(Context.RequestAborted));
                throw new HttpRequestException("An error occurred while retrieving the user profile.");
            }

            using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted));

            var principal = new ClaimsPrincipal(identity);
            var context = new OAuthCreatingTicketContext(principal, properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);
            context.RunClaimActions();

            // When the email address is not public, retrieve it from
            // the emails endpoint if the user:email scope is specified.
            if (!string.IsNullOrEmpty(Options.UserEmailsEndpoint) &&
                !identity.HasClaim(claim => claim.Type == ClaimTypes.Email) &&
                Options.Scope.Contains("user:email"))
            {
                var address = await GetValidEmailAsync(tokens);

                if (address is not null && !string.IsNullOrWhiteSpace(address.Email))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Email, address.Email, ClaimValueTypes.String, Options.ClaimsIssuer));
                    identity.AddClaim(new Claim(UserClaimTypes.EmailVerified, address.Verified.ToString().ToLower(), ClaimValueTypes.String, Options.ClaimsIssuer));
                }
            }

            await Events.CreatingTicket(context);
            return new AuthenticationTicket(context.Principal!, context.Properties, Scheme.Name);
        }

        private async Task<GitEmail?> GetValidEmailAsync(OAuthTokenResponse tokens)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, Options.UserEmailsEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            using var response = await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogWarning(
                    "An error occurred while retrieving the email address associated with the logged in user: the remote server returned a {Status} response with the following payload: {Headers} {Body}.",
                    response.StatusCode, response.Headers, await response.Content.ReadAsStringAsync(Context.RequestAborted));
                throw new HttpRequestException("An error occurred while retrieving the email address associated to the user profile.");
            }

            using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted));

            return (from address in payload.RootElement.EnumerateArray()
                    where address.GetProperty("primary").GetBoolean()
                    select JsonSerializer.Deserialize<GitEmail>(address.ToString())).FirstOrDefault();
        }
    }
}
