using Microsoft.FluentUI.AspNetCore.Components;
using Luval.SnapBite.Fluent.Components;
using Luval.AuthMate.OAuth;
using Luval.AuthMate.Postgres;
using Luval.AuthMate;
using Microsoft.AspNetCore.Authentication.OAuth;
using SnapBite;
using Luval.SnapBite.Fluent;
using System.Security.Claims;
using Luval.AuthMate.Entities;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Luval.AuthMate.Blazor;


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

// AuthMate: Configure the database implementation
var dbContext = 
    new PostgresAuthMateContext(ConfigHelper.GetValueAsString("ConnectionString:Authorization")); //postgres implementaion

// AuthMate: Creates an instance of the service
var authService = new AuthMateService(
        dbContext //provides the database context
    );


builder.Services.AddScoped<IAuthMateContext>((o) => new PostgresAuthMateContext(ConfigHelper.GetValueAsString("ConnectionString:Authorization")));
builder.Services.AddScoped<IAuthMateService, AuthMateService>();

// AuthMate: Function to be called after the user is authorized by Google
Func<OAuthCreatingTicketContext, Task> onTicket = async context =>
{

    //Checks for the user in the database and performs other validations, see the implementation here
    //https://github.com/marinoscar/AuthMate/blob/64b55c66f8bcd2534b5f8d8e02d1c3d1a439a9ef/src/Luval.AuthMate/AuthMateService.cs#L306
    
    DeviceInfo deviceInfo = null;

    if (context.Properties != null && 
        context.Properties.Items != null && 
        context.Properties.Items.ContainsKey("deviceInfo") &&
        !string.IsNullOrWhiteSpace(context.Properties.Items["deviceInfo"]))
            deviceInfo = DeviceInfo.FromString(context.Properties.Items["deviceInfo"]);

    await authService.UserAuthorizationProcessAsync(context.Identity, (u, c) =>
    {
        if (Debugger.IsAttached)
            Console.WriteLine($"User Id: {u.Id} Email {u.Email} Provider Key: {u.ProviderKey}");

    }, deviceInfo, CancellationToken.None);


};

// AuthMate: Add Presenters
builder.Services.AddAuthMateBlazorPresenters();

// AuthMate: Add Google Authentication configuration
builder.Services.AddGoogleAuth(new GoogleOAuthConfiguration()
{
    // client id from your config file
    ClientId = ConfigHelper.GetValueAsString("Authentication:Google:ClientID"),
    // the client secret from your config file
    ClientSecret = ConfigHelper.GetValueAsString("Authentication:Google:ClientSecret"),
    OnCreatingTicket = onTicket // function call
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
