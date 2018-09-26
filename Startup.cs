using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace ProfilesApp
{
    public static class Configuration
    {
        private static IConfiguration _configuration;
        public static void Set(IConfiguration configuration) {
            _configuration = configuration;
        }
        public static IConfiguration Get() {
            return _configuration;
        }
    }
    // delegates which control app configuration
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            ProfilesApp.Configuration.Set(configuration);
        }

        // add services to container
        public void ConfigureServices(IServiceCollection services)
        {
            // router
						ConfigureRenderingServices(services, "");
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddRouting();
						services.AddAuthentication(options => {
								options.DefaultAuthenticateScheme =
                  CookieAuthenticationDefaults.AuthenticationScheme;
								options.DefaultSignInScheme =
                  CookieAuthenticationDefaults.AuthenticationScheme;
								options.DefaultChallengeScheme =
                  OpenIdConnectDefaults.AuthenticationScheme;
						})
                .AddCookie()
                .AddOpenIdConnect(options => {
                    // `dotnet user-secrets set 'Azure:DirectoryV2:ClientId' '<client_id>'
                    // `dotnet user-secrets set 'Azure:DirectoryV2:ClientSecret' '<client_secret>'
                    options.ClientId =
                      ProfilesApp.Configuration.Get()["Azure:DirectoryV2:ClientId"];
                    options.ClientSecret =
                      ProfilesApp.Configuration.Get()["Azure:DirectoryV2:ClientSecret"];
                    options.CallbackPath = new PathString("/signin");
                    options.MetadataAddress = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";
                    // issuer: https://login.microsoftonline.com/9188040d-6c67-4c5b-b112-36a304b66dad/v2.0
                    options.TokenValidationParameters.ValidateIssuer = false;
                    options.Scope.Add("User.Read");
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("offline_access");
                });
        }

				// configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<IWebHost>>();
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // setup router
            // default handler
            var routeBuilder = new RouteBuilder(app, new RouteHandler(Server.DefaultRequestDelegate));
            routeBuilder.MapGet("hello", Server.RenderProfilePage);
						routeBuilder.MapPost("hello", Server.PostProfileForm);
            routeBuilder.MapRoute("root", "/");

            var routes = routeBuilder.Build();
            app.UseAuthentication();
            app.UseRouter(routes);
        }

        private static void ConfigureRenderingServices(IServiceCollection services, string customApplicationBasePath)
        {
            string applicationName;
            IFileProvider fileProvider;
            if (!string.IsNullOrEmpty(customApplicationBasePath))
            {
                applicationName = Path.GetFileName(customApplicationBasePath);
                fileProvider = new PhysicalFileProvider(customApplicationBasePath);
            }
            else
            {
                applicationName = Assembly.GetEntryAssembly().GetName().Name;
                fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            }

            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment
            {
                ApplicationName =  applicationName,
                WebRootFileProvider = fileProvider,
            });
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.FileProviders.Add(fileProvider);
            });
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddLogging();
            services.AddMvc();
        }
    }
}
