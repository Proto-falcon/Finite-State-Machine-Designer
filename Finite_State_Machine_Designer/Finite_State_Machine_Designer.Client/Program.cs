using Finite_State_Machine_Designer.Client;
using Finite_State_Machine_Designer.Client.FSM;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddScoped<IFiniteStateMachine, FiniteStateMachine>();
builder.Services.AddScoped<IFSMDrawer, FSMDrawer>();

await builder.Build().RunAsync();
