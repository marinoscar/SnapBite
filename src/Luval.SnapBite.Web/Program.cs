using Luval.SnapBite.Web.Components;
using SnapBite;

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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }

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
