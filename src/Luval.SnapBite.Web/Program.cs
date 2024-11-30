using Luval.AuthMate;
using Luval.AuthMate.OAuth;
using Luval.AuthMate.Postgres;
using Luval.SnapBite.Web.Components;
using Microsoft.AspNetCore.Authentication.OAuth;
using SnapBite;
using System.Security.Claims;

namespace Luval.SnapBite.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LoadConfig();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();


            /* BEGIN CUSTOM CONFIGURATION */

            // Add controllers
            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            // Configure AuthMate
            var dbContext = new PostgresAuthMateContext(ConfigHelper.GetValueAsString("ConnectionString:Authorization"));
            
            var authService = new AuthMateService(
                    dbContext,
                    "Free", "Administrator"
                );

            Func<OAuthCreatingTicketContext, Task> onTicket = async contex =>
            {
                await authService.OnUserAuthorizedAsync(contex.Identity, "Google", null);
            };

            // Add Google Authentication
            builder.Services.AddGoogleAuth(new GoogleOAuthConfiguration() { 
                ClientId = ConfigHelper.GetValueAsString("Authentication:Google:ClientID"),
                ClientSecret = ConfigHelper.GetValueAsString("Authentication:Google:ClientSecret"),
                OnCreatingTicket = onTicket
            });

            /* END CUSTOM CONFIGURATION */

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            /*******/
            app.MapControllers();
            app.MapBlazorHub();
            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();
            /*******/

            app.UseStaticFiles();
            app.UseAntiforgery();

           

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }

        /// <summary>
        /// Reads the configuration files
        /// </summary>
        private static void LoadConfig()
        {
            var envVar = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var isDev = (string.IsNullOrWhiteSpace(envVar) || envVar == "Development");

            var fileName = "{type}{env}.json";
            var location = Path.Combine(AppContext.BaseDirectory, fileName);
            var env = isDev ? ".debug" : "";

            ConfigHelper.AddJsonDocument(File.ReadAllText(location.Replace("{type}", "config").Replace("{env}", env)));
            ConfigHelper.AddJsonDocument(File.ReadAllText(location.Replace("{type}", "private").Replace("{env}", env)));

        }
    }
}
