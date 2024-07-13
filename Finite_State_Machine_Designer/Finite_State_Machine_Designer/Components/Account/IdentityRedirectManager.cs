using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;

namespace Finite_State_Machine_Designer.Components.Account
{
    internal sealed class IdentityRedirectManager(NavigationManager navigationManager)
    {
        public const string StatusCookieName = "Identity.StatusMessage";

        private static readonly CookieBuilder StatusCookieBuilder = new()
        {
            HttpOnly = true,
            IsEssential = true,
            MaxAge = TimeSpan.FromSeconds(5),
        };

        [DoesNotReturn]
        public void RedirectTo(string? uri)
        {
            uri ??= "";

            // Prevent open redirects.
            if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
            {
                uri = navigationManager.ToBaseRelativePath(uri);
            }

            /// During static rendering, NavigateTo throws a NavigationException
            /// which is handled by the framework as a redirect.
            /// So as long as this is called from a 
            /// statically rendered Identity component,
            /// the InvalidOperationException is never thrown.
            navigationManager.NavigateTo(uri);
            throw new InvalidOperationException($"{nameof(IdentityRedirectManager)} "
                + "can only be used during static rendering.");
        }

        [DoesNotReturn]
        public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
        {
            var uriWithoutQuery = navigationManager
                .ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
            var newUri = navigationManager
                .GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
            RedirectTo(newUri);
        }

        [DoesNotReturn]
        public void RedirectToWithStatus(
            string uri, string message, HttpContext context,
            SameSiteMode mode = SameSiteMode.Strict)
        {
            CookieOptions cookie = StatusCookieBuilder.Build(context);
            cookie.SameSite = mode;
            context.Response.Cookies.Append(StatusCookieName, message, cookie);
            RedirectTo(uri);
        }

        private string CurrentPath => navigationManager
            .ToAbsoluteUri(navigationManager.Uri).GetLeftPart(UriPartial.Path);

        [DoesNotReturn]
        public void RedirectToCurrentPage() => RedirectTo(CurrentPath);

        [DoesNotReturn]
        public void RedirectToCurrentPageWithStatus(
            string message, HttpContext context, 
            SameSiteMode mode = SameSiteMode.Strict)
            => RedirectToWithStatus(CurrentPath, message, context, mode);
    }
}
