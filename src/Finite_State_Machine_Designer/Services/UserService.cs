﻿using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Finite_State_Machine_Designer.Services
{

    public class UserService
    {
        private ClaimsPrincipal currentUser = new(new ClaimsIdentity());

        public ClaimsPrincipal GetUser()
        {
            return currentUser;
        }

        internal void SetUser(ClaimsPrincipal user)
        {
            if (currentUser != user)
            {
                currentUser = user;
            }
        }
    }

    internal sealed class UserCircuitHandler(
            AuthenticationStateProvider authenticationStateProvider,
            UserService userService) : CircuitHandler, IDisposable
    {

        public override Task OnCircuitOpenedAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            authenticationStateProvider.AuthenticationStateChanged +=
                AuthenticationChanged;

            return base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }

        private void AuthenticationChanged(Task<AuthenticationState> task)
        {
            _ = UpdateAuthentication(task);

            async Task UpdateAuthentication(Task<AuthenticationState> task)
            {
                try
                {
                    var state = await task;
                    userService.SetUser(state.User);
                }
                catch
                {
                }
            }
        }

        public override async Task OnConnectionUpAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            userService.SetUser(state.User);
        }

        public void Dispose()
        {
            authenticationStateProvider.AuthenticationStateChanged -=
                AuthenticationChanged;
        }
    }
}
