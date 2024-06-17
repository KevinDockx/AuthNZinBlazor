using BlazorAuthNZDemo.AuthorizationPolicies;
using BlazorAuthNZDemo.Client.Services;
using BlazorAuthNZDemo.Components;
using BlazorAuthNZDemo.HelperEvents;
using BlazorAuthNZDemo.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;

const string entraIdScheme = "EntraIDOpenIDConnect";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

// register server-based implementation to integrate with an API
builder.Services.AddScoped<IApiService, ServerApiService>();
builder.Services.AddScoped<BandsRepository>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddOpenIdConnectAccessTokenManagement()
    .AddBlazorServerAccessTokenManagement<CustomServerSideTokenStore>();
builder.Services.AddTransient<CustomTokenStorageOidcEvents>();

//builder.Services.AddHttpClient("LocalAPIClient", cfg =>
//    {
//        cfg.BaseAddress = new Uri(
//            builder.Configuration["LocalAPIBaseAddress"] ??
//                throw new Exception("LocalAPIBaseAddress is missing."));
//    });
builder.Services.AddHttpClient("RemoteAPIClient", cfg =>
     {
         cfg.BaseAddress = new Uri(
            builder.Configuration["RemoteAPIBaseAddress"] ??
                throw new Exception("RemoteAPIBaseAddress is missing."));
     }).AddUserAccessTokenHandler();

builder.Services.AddAuthentication(options =>
{ 
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = entraIdScheme;
}).AddOpenIdConnect(entraIdScheme, oidcOptions =>
{
    oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    oidcOptions.Authority = "https://login.microsoftonline.com/5c154a7e-0c13-4f92-8531-e3f4d8fbeae9/v2.0";
    //oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
    //oidcOptions.UsePkce = true;
    oidcOptions.Scope.Add(OpenIdConnectScope.OpenIdProfile);
    oidcOptions.Scope.Add("api://c34d8b8f-09b0-4889-8444-0bd3b1c588af/FullAccess");
    oidcOptions.Scope.Add("offline_access");
    //oidcOptions.CallbackPath = new PathString("/signin-oidc");
    //oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
    //oidcOptions.ClientId = "input-your-client-id-here";
    //oidcOptions.ClientSecret = "input-your-secret-here";
    oidcOptions.MapInboundClaims = false;
    oidcOptions.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
    oidcOptions.TokenValidationParameters.RoleClaimType = "role";
    oidcOptions.SaveTokens = true;
    oidcOptions.EventsType = typeof(CustomTokenStorageOidcEvents);

}).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, 
    options =>
    {
        options.AccessDeniedPath = new PathString("/AccessDenied"); 
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.IsFromBelgium,
        Policies.IsFromBelgiumPolicy());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorAuthNZDemo.Client._Imports).Assembly);

// define a local API for testing
app.MapGet("localapi/bands", (BandsRepository bandsRepository) =>
{
    return Results.Ok(bandsRepository.GetBands());    
}).RequireAuthorization(Policies.IsFromBelgiumPolicy());

app.MapGet("/login", (string? returnUrl, HttpContext httpContext) =>
{
    // ensure the returnUrl is valid & safe.  
    returnUrl = ValidateUri(httpContext, returnUrl);     

    return TypedResults.Challenge(
                 new AuthenticationProperties
                 { RedirectUri = returnUrl });
}).AllowAnonymous();

app.MapPost("/logout", ([FromForm] string? returnUrl, HttpContext httpContext) =>
{
    returnUrl = ValidateUri(httpContext, returnUrl);
    return TypedResults.SignOut(
        new AuthenticationProperties
        { RedirectUri = returnUrl },
            [CookieAuthenticationDefaults.AuthenticationScheme, entraIdScheme]);
});

app.MapGet("/forward-to-remote-api/bands", async (IApiService apiService) =>
{
    return Results.Ok(await apiService.CallRemoteApiAsync());
}).RequireAuthorization(Policies.IsFromBelgiumPolicy());

app.Run();

public partial class Program
{
    private static string ValidateUri(HttpContext httpContext, string? uri)
    {
        string basePath = string.IsNullOrEmpty(httpContext.Request.PathBase)
                ? "/" : httpContext.Request.PathBase;

        if (string.IsNullOrEmpty(uri))
        {
            return basePath;
        }
        else if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            return new Uri(uri, UriKind.Absolute).PathAndQuery;
        }
        else if (uri[0] != '/')
        {
            return $"{basePath}{uri}";
        }

        return uri;
    }
}