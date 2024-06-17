using BlazorAuthNZDemo.Client.Pages;
using BlazorAuthNZDemo.Client.Services;
using BlazorAuthNZDemo.Components;
using BlazorAuthNZDemo.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// register server-based implementation to integrate with an API
builder.Services.AddScoped<IApiService, ServerApiService>();

builder.Services.AddHttpClient("LocalAPIClient", cfg =>
    {
        cfg.BaseAddress = new Uri(
            builder.Configuration["LocalAPIBaseAddress"] ??
                throw new Exception("LocalAPIBaseAddress is missing."));
    });
builder.Services.AddHttpClient("RemoteAPIClient", cfg =>
     {
         cfg.BaseAddress = new Uri(
            builder.Configuration["RemoteAPIBaseAddress"] ??
                throw new Exception("RemoteAPIBaseAddress is missing."));
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

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorAuthNZDemo.Client._Imports).Assembly);

// define a local API for testing
app.MapGet("localapi/bands", () =>
{
    return Results.Ok(new[] { new { Id = 1, Name = "Nirvana (from local API)" },
        new { Id = 2, Name = "Queens of the Stone Age (from local API)" },
        new { Id = 3, Name = "Fred Again. (from local API)" },
        new { Id = 4, Name = "Underworld (from local API)" } });    
});

app.Run();
