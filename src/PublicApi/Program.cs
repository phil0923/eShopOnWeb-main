using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalApi.Endpoint.Extensions;
using Microsoft.eShopWeb.PublicApi;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;
using Infrastructure.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.PublicApi.Catalog;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5099");

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// MVC / Swagger
builder.Services.AddEndpoints();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddSingleton<IUriComposer, NoOpUriComposer>();

var provider = builder.Configuration.GetValue<string>("CatalogProvider") ?? "Ef";

if (provider.Equals("Ef", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddCatalogDb(builder.Configuration);
    builder.Services.AddScoped<ICatalogFacade, CatalogFacadeEf>();
}
else if (provider.Equals("Http", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.Configure<HttpCatalogFacade.CatalogServiceOptions>(
        builder.Configuration.GetSection("CatalogService"));

    builder.Services.AddHttpClient<HttpCatalogFacade>();
    builder.Services.AddScoped<ICatalogFacade, HttpCatalogFacade>();
}
else
{
    throw new InvalidOperationException($"Unknown CatalogProvider '{provider}'. Use 'Ef' or 'Http'.");
}

var app = builder.Build();

app.Logger.LogInformation("PublicApi booting...");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (provider.Equals("Ef", StringComparison.OrdinalIgnoreCase))
{
    using var scope = app.Services.CreateScope();
    var ctx = scope.ServiceProvider.GetRequiredService<CatalogContext>();

    await ctx.Database.MigrateAsync();
    try
    {
        await CatalogContextSeed.SeedAsync(ctx, app.Logger);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Catalog seeding failed");
    }
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PublicApi v1"));

// Endpoints
app.MapGet("/health", () => Results.Ok(new { status = "ok", provider }));
app.MapEndpoints();
app.MapControllers();

app.Logger.LogInformation("PublicApi listening on http://localhost:5099 (provider={Provider})", provider);
app.Run();
