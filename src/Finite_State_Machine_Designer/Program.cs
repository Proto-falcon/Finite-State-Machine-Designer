using Finite_State_Machine_Designer.Client.Pages;
using Finite_State_Machine_Designer.Components;
using Finite_State_Machine_Designer.Components.Account;
using Finite_State_Machine_Designer.Configuration;
using Finite_State_Machine_Designer.Configuration.ExternalLogins;
using Finite_State_Machine_Designer.Data;
using Finite_State_Machine_Designer.Data.Identity;
using Finite_State_Machine_Designer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
});

builder.Services
    .Configure<EmailServiceConfig>(
        builder.Configuration.GetSection("FSM:EmailServiceConfig")
    )
    .Configure<EmailContentPaths>(
        builder.Configuration.GetSection("EmailContentPaths")
    )
    .Configure<UsersConfig>(
        builder.Configuration.GetSection("UsersConfig")
    );

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider,
    PersistingServerAuthenticationStateProvider>();
builder.Services.AddScoped<UserService>();
builder.Services.TryAddEnumerable(
    ServiceDescriptor.Scoped<CircuitHandler, UserCircuitHandler>());
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<DataProtectionContext>();

builder.Services.AddAuthorization();
AuthenticationBuilder authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    });

ExternalAuths externalAuths = new();
builder.Configuration.GetSection("FSM:ExternalAuths")
    .Bind(externalAuths);

string accessDeniedPath = "/Account/Login";

ExternalGoogle.Setup(externalAuths.GoogleAuth, authBuilder, accessDeniedPath);
ExternalSlack.Setup(externalAuths.SlackAuth, authBuilder, accessDeniedPath);
ExternalGithub.Setup(externalAuths.GithubAuth, authBuilder, accessDeniedPath);
ExternalDiscord.Setup(externalAuths.DiscordAuth, authBuilder, accessDeniedPath);

authBuilder.AddIdentityCookies();

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");
var dppConnString = builder.Configuration.GetConnectionString("DPPConnection")
    ?? throw new InvalidOperationException("Connection string 'DPPConnection' not found.");
builder.Services.AddDbContextFactory<DataProtectionContext>(options =>
    options.UseSqlServer(dppConnString,
    x => x.MigrationsAssembly("Finite_State_Machine_Designer.Data")
        .EnableRetryOnFailure())
);
builder.Services.AddDbContextFactory<ApplicationDbContext>(options => 
    options.UseSqlServer(connectionString,
    x => x.MigrationsAssembly("Finite_State_Machine_Designer.Data")
    .EnableRetryOnFailure())
);

builder.Services.AddIdentityCore<ApplicationUser>(
        options => {
            options.SignIn.RequireConfirmedAccount = true;
            })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<SmtpFactory>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>,
    IdentityEmailSender>();
builder.Services.AddHsts(options =>
{
    options.MaxAge = TimeSpan.FromDays(1);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    /// The default HSTS value is 30 days.
    /// You may want to change this for production scenarios,
    /// see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(FSMDesigner).Assembly);

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
