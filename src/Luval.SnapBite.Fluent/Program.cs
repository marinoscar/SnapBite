using Microsoft.FluentUI.AspNetCore.Components;
using Luval.SnapBite.Fluent.Components;
using Luval.AuthMate.Postgres;
using Microsoft.AspNetCore.Authentication.OAuth;
using SnapBite;
using Luval.SnapBite.Fluent;
using System.Security.Claims;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Luval.AuthMate.Blazor;
using Luval.AuthMate.Core.Entities;
using Luval.AuthMate.Infrastructure.Data;
using Luval.AuthMate.Core.Interfaces;
using Luval.AuthMate.Core;
using Luval.AuthMate.Infrastructure.Configuration;
using Luval.AuthMate.Core.Services;


ConfigHelper.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

// AuthMate: Add support for controllers
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

var context = new PostgresAuthMateContext(ConfigHelper.GetValueAsString("ConnectionString:Authorization"));
context.InitializeDbAsync("oscar.marin.saenz@gmail.com").Wait();

// AuthMate: add the AuthMate Services
builder.Services.AddAuthMateServices((f) => {
    //Configures database connection
    return new PostgresAuthMateContext(ConfigHelper.GetValueAsString("ConnectionString:Authorization"));
});


// AuthMate: Add Presenters
builder.Services.AddAuthMateBlazorPresenters();

// AuthMate: Add Google Authentication configuration
builder.Services.AddAuthMateGoogleAuth(new GoogleOAuthConfiguration()
{
    // client id from your config file
    ClientId = ConfigHelper.GetValueAsString("Authentication:Google:ClientID"),
    // the client secret from your config file
    ClientSecret = ConfigHelper.GetValueAsString("Authentication:Google:ClientSecret"),
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

/*** AuthMate: Additional configuration  ****/
app.MapControllers();
app.UseRouting();
app.UseAuthorization();
app.UseAuthentication();
/*** AuthMate:                           ****/

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
