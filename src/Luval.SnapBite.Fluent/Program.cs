using Microsoft.FluentUI.AspNetCore.Components;
using Luval.SnapBite.Fluent.Components;
using Luval.AuthMate.OAuth;
using Luval.AuthMate.Postgres;
using Luval.AuthMate;
using Microsoft.AspNetCore.Authentication.OAuth;
using SnapBite;
using Luval.SnapBite.Fluent;
using System.Security.Claims;


ConfigHelper.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

// Add controllers
builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
// Configure AuthMate
var dbContext = new PostgresAuthMateContext(ConfigHelper.GetValueAsString("ConnectionString:Authorization"));
var authService = new AuthMateService(
        dbContext
    );
// Function to be called after the user is authorized by Google
Func<OAuthCreatingTicketContext, Task> onTicket = async context =>
{
    //Add information about the google provider
    context.Identity.AddClaim(new Claim("AppUserProviderType", "Google"));

    if (context.Identity.IsPowerUser())
        await authService.RegisterAdminUserAsync(context.Identity.ToUser(), 
            ConfigHelper.GetValueAsString("Authentication:DefaultAccountType"));
    
};
// Add Google Authentication configuration
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

/*** Adds support for controllers     ****/
app.MapControllers();
app.UseRouting();
app.UseAuthorization();
app.UseAuthentication();
/*** End code to suupport controllers ****/

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
