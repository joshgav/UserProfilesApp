using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProfilesApp {
    public class Server {

        // implements Microsoft.AspNetCore.Http.RequestDelegate
        public static async Task DefaultRequestDelegate(HttpContext context) {
            await context.Response.WriteAsync("Hello!");
        }

        public static async Task RenderProfilePage(HttpContext context) {
            var logger = context.RequestServices.GetRequiredService<ILogger<IWebHost>>();
            if (!context.User.Identity.IsAuthenticated) {
                await context.ChallengeAsync();
            } else {
                var principal = context.User as ClaimsPrincipal;
                if (null != principal) {
                    var name = principal.FindFirst("name");
                    logger.LogInformation($"name claim: [{name}]");
                    await context.Response.WriteAsync($"Hi {name.Value}! You've been authenticated.");
                    foreach (Claim claim in principal.Claims) {
                        logger.LogInformation($"claim: [{claim}]");
                    }
                }
            }
        }
    }
}
