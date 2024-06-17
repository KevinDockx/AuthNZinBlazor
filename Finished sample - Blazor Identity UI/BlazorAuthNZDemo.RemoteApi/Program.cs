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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowWASMOrigin");

app.UseHttpsRedirection();

// define an API for testing
app.MapGet("remoteapi/bands", () =>
{
    return Results.Ok(new[] { new { Id = 1, Name = "Arctic Monkeys (from remote API)" },
        new { Id = 2, Name = "Nine Inch Nails (from remote API)" },
        new { Id = 3, Name = "Bruce Springsteen (from remote API)" },
        new { Id = 4, Name = "Fleetwood Mac (from remote API)" } });
});

app.Run();
 