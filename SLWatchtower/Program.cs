using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SLWatchtower.Data;

var builder = WebApplication.CreateBuilder(args);

var Configuration = builder.Configuration;

var auth0Domain = Configuration.GetValue<string>("Auth0:Domain");
var auth0ClientId = Configuration.GetValue<string>("Auth0:ClientId");
var auth0ClientSecret = Configuration.GetValue<string>("Auth0:ClientSecret");

var connectionString = Configuration.GetConnectionString("Storage");
var databaseName = Configuration.GetValue<string>("DatabaseName");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();


app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
