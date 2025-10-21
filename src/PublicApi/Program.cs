using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MinimalApi.Endpoint.Extensions;
using Microsoft.eShopWeb.PublicApi;
using Microsoft.eShopWeb.Infrastructure;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Catalog.Abstractions;
using Infrastructure.Catalog;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5099");

// Minimal logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// --- Services ---
builder.Services.AddEndpoints();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Health + endpoints
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapEndpoints();
app.MapControllers();

app.Logger.LogInformation("PublicApi listening on http://localhost:5099");
app.Run();
