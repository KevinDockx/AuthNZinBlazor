using BlazorAuthNZDemo.AuthorizationPolicies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow requests from the Blazor WASM host from JavaScript (JS interop is used
// by WASM) 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWASMOrigin",
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:7224",
                            "http://localhost:5097");
                      });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/5c154a7e-0c13-4f92-8531-e3f4d8fbeae9/v2.0";
        options.Audience = "api://c34d8b8f-09b0-4889-8444-0bd3b1c588af";
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role",
            ValidIssuer = "https://sts.windows.net/5c154a7e-0c13-4f92-8531-e3f4d8fbeae9/"
        };
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
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowWASMOrigin");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

// define an API for testing
app.MapGet("remoteapi/bands", () =>
{
    return Results.Ok(new[] { new { Id = 1, Name = "Arctic Monkeys (from remote API)" },
        new { Id = 2, Name = "Nine Inch Nails (from remote API)" },
        new { Id = 3, Name = "Bruce Springsteen (from remote API)" },
        new { Id = 4, Name = "Fleetwood Mac (from remote API)" } });
}).RequireAuthorization(Policies.IsFromBelgiumPolicy());

app.Run();
 