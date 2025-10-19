using BlockedCountriesApi.Repositories;
using BlockedCountriesApi.Repositories.Interfaces;
using BlockedCountriesApi.Services;
using BlockedCountriesApi.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register repositories as Singletons (in-memory storage)
builder.Services.AddSingleton<IBlockedCountryRepository, BlockedCountryRepository>();
builder.Services.AddSingleton<IBlockedAttemptLogRepository, BlockedAttemptLogRepository>();

// Register HttpClient for GeolocationService
builder.Services.AddHttpClient<IGeolocationService, GeolocationService>();

// Register background service for temporal block cleanup
builder.Services.AddHostedService<TemporalBlockCleanupService>();

builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Blocked Countries API",
        Description = "An ASP.NET Core Web API for managing blocked countries and validating IP addresses using geolocation",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@blockedcountriesapi.com"
        }
    });

    // Include XML comments for better documentation
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Blocked Countries API v1");
        options.DocumentTitle = "Blocked Countries API";
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
