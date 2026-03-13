using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using ButterShop.ApiService.Data;
using ButterShop.ApiService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.AddNpgsqlDbContext<AppDbContext>("shopdb");

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "ButterShop API Reference";
    options.Servers = [];
    options.Theme = ScalarTheme.BluePlanet;
    options.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    for (var i = 0; i < 10; i++)
    {
        try
        {
            await db.Database.MigrateAsync();
            break;
        }
        catch
        {
            await Task.Delay(2000);
        }
    }
}

app.MapProductEndpoints();
app.MapOrderEndpoints();
app.MapDefaultEndpoints();

app.Run();
