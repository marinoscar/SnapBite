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
            var authService = new AuthMateService(
                    new PostgresAuthMateContext(ConfigHelper.GetValueAsString("ConnectionString:Authorization")),
                    "Free", "Administrator"
                );

            Func<OAuthCreatingTicketContext, Task> onTicket = async contex =>
            {
                var appUser = contex.HttpContext.User.ToUser();
                var newUser = await authService.CreateUserAsAdminAsync(appUser);
                //add full json object
                contex.Identity?.AddClaim(new Claim("appuser", newUser.ToString()));
                //add roles
                foreach (var role in newUser.UserRoles)
                {
                    if(role != null && role.Role != null)
                        contex.Identity?.AddClaim(new Claim(ClaimTypes.Role, role.Role.Name));
                }
                // id, account and others
                contex.Identity?.AddClaim(new Claim("AppUserId", newUser.Id.ToString()));
                var account = newUser.UserInAccounts.ToList().FirstOrDefault();
                if(account != null)
                    contex.Identity?.AddClaim(new Claim("AppUserAccountId", newUser.UserInAccounts.First().AccountId.ToString()));
                // account Active
                var activeDate = newUser.UtcActiveUntil ?? DateTime.UtcNow.AddYears(5);
                contex.Identity?.AddClaim(new Claim("AppUserUtcActiveUntil", activeDate.ToString("u")));

                return;
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
