using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using MinimalApi.Endpoint.Extensions;

using Microsoft.eShopWeb.PublicApi;                              // MappingProfile (hvis du har den)
using Microsoft.eShopWeb.Infrastructure;                         // AddCatalogDb, CatalogSettings, UriComposer
using Microsoft.eShopWeb.Infrastructure.Data;                    // CatalogContext, CatalogContextSeed
using Microsoft.eShopWeb.ApplicationCore.Interfaces;             // IUriComposer
using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;   // ICatalogFacade
using Infrastructure.Catalog;                                     // CatalogFacadeEf

var builder = WebApplication.CreateBuilder(args);

// Kør kun HTTP i dev
builder.WebHost.UseUrls("http://localhost:5099");

// Minimal logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// --- Services ---
builder.Services.AddEndpoints();                    // scanner MinimalApi endpoints (IEndpoint)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// AutoMapper (hvis MappingProfile findes i PublicApi)
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

// Catalog DB (InMemory når UseOnlyInMemoryDatabase=true)
builder.Services.AddCatalogDb(builder.Configuration);

// Facade (EF-implementation)
builder.Services.AddScoped<ICatalogFacade, CatalogFacadeEf>();
builder.Services.AddSingleton<IUriComposer, NoOpUriComposer>();


var app = builder.Build();

app.Logger.LogInformation("PublicApi booting...");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var ctx = sp.GetRequiredService<CatalogContext>();
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

// Health + endpoints
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapEndpoints();
app.MapControllers();

app.Logger.LogInformation("PublicApi listening on http://localhost:5099");
app.Run();
