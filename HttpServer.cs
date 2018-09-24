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

        private const string nameClaimType = "name";
        private const string preferredUsernameClaimType = "preferred_username";
        private const string oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        private const string tenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private const string nameIdentifierClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string emailClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

        // implements Microsoft.AspNetCore.Http.RequestDelegate
        public static async Task DefaultRequestDelegate(HttpContext context) {
            await context.Response.WriteAsync("Hello!");
        }

        // implements Microsoft.AspNetCore.Http.RequestDelegate
        public static async Task RenderProfilePage(HttpContext context) {
            ClaimsPrincipal principal;
            var logger = context.RequestServices.GetRequiredService<ILogger<IWebHost>>();

            if (!context.User.Identity.IsAuthenticated) {
                await context.ChallengeAsync();
                return;
            }

            principal = context.User as ClaimsPrincipal;
            if (null != principal) {
                var name = principal.FindFirst("name");
                logger.LogInformation($"name claim: [{name}]");
                await context.Response.WriteAsync($"Hi {name.Value}! You've been authenticated.");
                foreach (Claim claim in principal.Claims) {
                    logger.LogInformation($"claim: [{claim}]");
                }
            }

            var idClaim = principal.FindFirst(nameIdentifierClaimType);

            var profiles = new UserProfilesCollection();
            await profiles.AddProfileAsync(new UserProfile() {
                Id = idClaim.Value,
                FullName = principal.FindFirst(nameClaimType).Value,
                Email = principal.FindFirst(emailClaimType).Value,
                ExternalIdentifier = idClaim.Value,
                Etag = ""
            });

            var currentProfile = await profiles.GetProfileAsync(idClaim.Value);
            logger.LogInformation($"got profileId {currentProfile.Id}, fullName {currentProfile.FullName}");

            return;

        }
    }
}
