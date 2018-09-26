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

namespace ProfilesApp
{
    public class Server
    {
        private const string nameClaimType = "name";
        private const string preferredUsernameClaimType = "preferred_username";
        private const string oidClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        private const string tenantIdClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
        private const string nameIdentifierClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private const string emailClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";

        public static async Task DefaultRequestDelegate(HttpContext context)
        {
            await context.Response.WriteAsync("Hello!");
        }

        public static async Task RenderProfilePage(HttpContext context)
        {
            var renderer = context.RequestServices.GetRequiredService<IViewRenderService>();
            UserProfile profile = await GetUserProfileFromContext(context);
            // await context.Response.WriteAsync($"Hi {profile.FullName}. You've been authenticated with identifier {profile.Id}.");
            await context.Response.WriteAsync(
                await renderer.RenderViewToStringAsync("Profile", profile));
        }

        public static async Task PostProfileForm(HttpContext context)
        {
            var principal = (ClaimsPrincipal)context.User;
            var idClaim = principal.FindFirst(nameIdentifierClaimType);
            var formData = context.Request.Form;
            var updatedProfile = new UserProfile() {
                Id = idClaim.Value,
                Etag = formData["etag"],
                FullName = formData["fullName"],
                Email = formData["email"],
            };

            // check for picture and upload to blob store
            if (formData.Files.Any()) {
                var stream = formData.Files.First().OpenReadStream();
                var pictures = new Pictures(context);
                var pictureUrl = await pictures.PutPicture(updatedProfile.Id, stream);
                updatedProfile.PictureUrl = await pictures.GetPictureUrl(updatedProfile.Id);
            }

            var profiles = new UserProfiles();
            await profiles.UpdateProfileAsync(updatedProfile);
            await RenderProfilePage(context);
            return;
        }

        public static async Task<UserProfile> GetUserProfileFromContext(HttpContext context)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<IWebHost>>();

            if (!context.User.Identity.IsAuthenticated) {
                await context.ChallengeAsync();
								return new UserProfile();
            }

            ClaimsPrincipal principal = (ClaimsPrincipal)context.User;
            var idClaim = principal.FindFirst(nameIdentifierClaimType);
            var nameClaim = principal.FindFirst(nameClaimType);
            logger.LogInformation($"[{nameClaim}, {idClaim}]");
            foreach (Claim claim in principal.Claims) {
                logger.LogInformation($"claim: [{claim}]");
            }

            var profiles = new UserProfiles();
            // first see if profile exists for this ID
            var profile = await profiles.GetProfileAsync(idClaim.Value);

            // if not, add a new profile for this ID
            if (profile == null) {
                await profiles.AddProfileAsync(new UserProfile() {
                    Id = idClaim.Value,
                    FullName = principal.FindFirst(nameClaimType).Value,
                    Email = principal.FindFirst(emailClaimType).Value,
                    ExternalIdentifier = idClaim.Value 
                });
                profile = await profiles.GetProfileAsync(idClaim.Value);
            }; 

            logger.LogInformation($"got profileId {profile.Id}, fullName {profile.FullName}");
            return profile;
        }
    }
}
