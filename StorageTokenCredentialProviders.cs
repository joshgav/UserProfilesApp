using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.WindowsAzure.Storage.Auth;

namespace ProfilesApp
{
    public interface IStorageTokenCredentialProvider
    {
        Task<TokenCredential> GetTokenCredentialAsync();
        System.Uri GetBlobEndpoint();
    }

    public class StorageEnvironmentTokenCredentialProvider : IStorageTokenCredentialProvider
    {
        private String clientId;
        private String clientSecret;
        private String redirectUri = "https://localhost:5001/signin";

        private String blobUri;
        private List<String> scopes = new List<String>() { "https://storage.azure.com/.default" };

        public async Task<TokenCredential> GetTokenCredentialAsync() {
            clientId = ProfilesApp.Configuration.Get()["AAD:ClientID"];
            clientSecret = ProfilesApp.Configuration.Get()["AAD:ClientSecret"];
            var clientCredential = new ClientCredential(clientSecret);

            var authentication = new ConfidentialClientApplication(
                clientId: clientId,
                redirectUri: redirectUri,
                clientCredential: clientCredential,
                userTokenCache: null,
                appTokenCache: null
            );
            var result = await authentication.AcquireTokenForClientAsync(scopes);

            var tokenCred = new TokenCredential(
                result.AccessToken,
                async (state, cancellationToken) => {
                    var newResult = await authentication.AcquireTokenForClientAsync(scopes);
                    return new NewTokenAndFrequency(newResult.AccessToken);
                },
                null,
                TimeSpan.FromSeconds(60)
            );
            return tokenCred;
        }

        public System.Uri GetBlobEndpoint() {
           blobUri = ProfilesApp.Configuration.Get()["Azure:Storage:Blob:Endpoint"]; 
           return new Uri(blobUri);
        }

        async Task<TokenCredential> IStorageTokenCredentialProvider.GetTokenCredentialAsync() {
            return await this.GetTokenCredentialAsync();
        }

        System.Uri IStorageTokenCredentialProvider.GetBlobEndpoint() {
            return this.GetBlobEndpoint();
        }
    }
}
