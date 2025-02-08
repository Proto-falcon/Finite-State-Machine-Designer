using Microsoft.AspNetCore.Identity;

namespace Finite_State_Machine_Designer.Components.Account
{
    public static class TokenManager
    {
        /// <summary>
        /// Adds the access token to the database
        /// </summary>
        /// <typeparam name="TUser">The type encapsulating a user.</typeparam>
        /// <param name="UserManager">
        /// Provides the API for managing users in a persistent store.
        /// </param>
        /// <param name="user">Chosen user.</param>
        /// <param name="externalLoginInfo">
        /// External login information about the user
        /// </param>
        /// <param name="Logger">Optional logger to log the events of the task.</param>
        /// <returns>
        /// <see langword="true"/> for when the access token is added successfully,
        /// otherwise <see langword="false"/> for not added.
        /// </returns>
        public async static Task<bool> AddAccessToken<TUser>(
            UserManager<TUser> UserManager, TUser user,
            ExternalLoginInfo externalLoginInfo, ILogger? Logger = null)
            where TUser : class
        {
            bool isAdded = false;
            if (externalLoginInfo.AuthenticationTokens is not null)
            {
                var accessToken = externalLoginInfo.AuthenticationTokens
                    .FirstOrDefault(token => token.Name.Contains("access_token"));

                if (accessToken is not null)
                {
                    var tokenResult = await UserManager
                        .SetAuthenticationTokenAsync(
                        user, externalLoginInfo.LoginProvider,
                        accessToken.Name, accessToken.Value);
                    foreach (var error in tokenResult.Errors)
                        Logger?.LogError("Set access Token Error: {Desc}",
                            error.Description);
                    isAdded = tokenResult.Succeeded;
                }
                else
                    isAdded = false;
            }
            else
                isAdded = false;
            return isAdded;
        }

        /// <summary>
        /// Removes the authentication token for user.
        /// </summary>
        /// <remarks>
        /// <para><b>Remarks:</b></para>
        /// Only supports revoking google access tokens.
        /// </remarks>
        /// <typeparam name="TUser">The type encapsulating a user.</typeparam>
        /// <param name="UserManager">
        /// Provides the API for managing users in a persistent store.
        /// </param>
        /// <param name="user">Chosen user.</param>
        /// <param name="LoginProvider">
        /// 3rd party that gave external login access to the app.
        /// </param>
        /// <param name="logger">
        /// Optional logger to log the events of the task.
        /// </param>
        /// <returns>
        /// <see langword="true"/> for when the access token is revoked
        /// and deleted from database, otherwise <see langword="false"/> when
        /// the access token isn't either revoked or deleted from database.
        /// </returns>
        public async static Task<bool> RemoveToken<TUser>(
            UserManager<TUser> UserManager, TUser user,
            string LoginProvider, ILogger? logger = null) 
            where TUser : class
        {
            string? accessToken = await UserManager
                .GetAuthenticationTokenAsync(user, LoginProvider!, "access_token");
            bool isRevoked = true;

            if (!string.IsNullOrWhiteSpace(accessToken))
                isRevoked = await RevokeGoogleAccessToken(accessToken, logger);

            if (!isRevoked)
                return false;
            IdentityResult result = await UserManager
                .RemoveAuthenticationTokenAsync(user, LoginProvider!, "access_token");
            
            if (!result.Succeeded)
            {
                logger?.LogError("Coudln't remove the access token");
                foreach (var error in result.Errors)
                    logger?.LogError("[{CODE}] {DESC}", error.Code, error.Description);
            }
            else
                logger?.LogInformation("Removed the google access token");

            return result.Succeeded;
        }

        /// <summary>
        /// Revokes the Google Access token.
        /// </summary>
        /// <param name="accessToken">Google Access token</param>
        /// <param name="logger">Logger to log any errors that may occur.</param>
        /// <returns><see langword="true"/> for successfull response, otherwise
        /// <see langword="false"/> for unsuccessfull response.
        /// </returns>
        private async static Task<bool> RevokeGoogleAccessToken(
            string accessToken, ILogger? logger = null)
        {
        using HttpClient revokeClient = new ();
            FormUrlEncodedContent content = new ([new ("token", accessToken)]);

            try
            {
            using HttpResponseMessage response =
                    await revokeClient
                    .PostAsync("https://oauth2.googleapis.com/revoke", content);
                if (!response.IsSuccessStatusCode)
                {
                    string errorResponse = await response.Content.ReadAsStringAsync();
                    logger?.LogError("Error response: {Msg}", errorResponse);
                }
                response.EnsureSuccessStatusCode();
                logger?.LogInformation("Revoked google access token");
            }
            catch (Exception ex)
            {
                logger?.LogError("Couldn't get the revocation response");
                logger?.LogError("{Error}", ex.ToString());
                return false;
            }
            return true;
        }
    }
}
