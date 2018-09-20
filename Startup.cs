using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProfilesApp
{
    // delegates which control app configuration
    public class Startup
    {
				public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // add services to container
        public void ConfigureServices(IServiceCollection services)
        {
            // router
            services.AddRouting();
						services.AddAuthentication(options => {
								options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
								options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
								options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
						})
                .AddCookie()
                .AddOpenIdConnect(options => {
                    // `dotnet user-secrets set 'AAD:ClientID' '<client_id>'
                    // `dotnet user-secrets set 'AAD:ClientSecret' '<client_secret>'
                    options.ClientId = Configuration["AAD:ClientId"];
                    options.ClientSecret = Configuration["AAD:ClientSecret"];
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
            routeBuilder.MapRoute("root", "/");

            var routes = routeBuilder.Build();
            app.UseAuthentication();
            app.UseRouter(routes);
        }
    }
}
