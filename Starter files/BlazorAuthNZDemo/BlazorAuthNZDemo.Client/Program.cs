using BlazorAuthNZDemo.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// register api service for calls originating from WASM
builder.Services.AddScoped<IApiService, ClientApiService>();

builder.Services.AddKeyedScoped<HttpClient>("LocalAPIClientFromWASM", 
    (sp, key) =>
       new HttpClient
       {
           BaseAddress = new Uri(builder.Configuration["LocalAPIBaseAddress"] ??
                throw new Exception("LocalAPIBaseAddress is missing."))
       });

builder.Services.AddKeyedScoped<HttpClient>("RemoteAPIClientFromWASM",
    (sp, key) =>
       new HttpClient
       {
           BaseAddress = new Uri(builder.Configuration["RemoteAPIBaseAddress"] ??
                throw new Exception("RemoteAPIBaseAddress is missing."))
       });

await builder.Build().RunAsync();
