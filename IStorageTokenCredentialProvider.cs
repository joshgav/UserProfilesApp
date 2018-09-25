using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProfilesApp
{
    public interface IStorageTokenCredentialProvider
    {
        TokenCredential GetTokenCredential();
        System.Uri GetBlobEndpoint();
    }

    public class StorageEnvironmentTokenCredentialProvider
    {

        private String clientId;
        private String clientSecret;
        private String blobUri;
        private String storageScope = "https://storage.azure.com/.default";
        private Jwt lastJwt;

        public TokenCredential GetTokenCredential() {
            clientId = ProfilesApp.Configuration.Get()["AAD:ClientID"];
            clientSecret = ProfilesApp.Configuration.Get()["AAD:ClientSecret"];
            lastJwt = AuthenticationContext.acquireTokenWithClientCredentials(
              storageScope, clientId, clientSecret);

            return new TokenCredential(
                lastJwt.AccessToken,
                (state, cancellationToken) => {
                  var newJwt =
                    AuthenticationContext
                      .acquireTokenWithRefreshToken(lastJwt.RefreshToken);
                  lastJwt = newJwt;
                  return newJwt.AccessToken;
                },
                null,
                lastJwt.RefreshToken.ExpiresOn - DateTime.Now - 120s // refresh freq
            )
        }

        public System.Uri GetBlobEndpoint() {
           blobUri = ProfilesApp.Configuration.Get()["Azure:Storage:Blob:Endpoint"]; 
           return new Uri(blobUri);
        }

        IStorageTokenCredentialProvider.GetTokenCredential() {
            return this.GetTokenCredential();
        }

        IStorageTokenCredentialProvider.GetBlobEndpoint() {
            return this.GetBlobEndpoint();
        }
    }
}
