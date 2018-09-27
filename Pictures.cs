using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ProfilesApp
{
    public class Pictures
    {
        private String picturesContainerName = "pictures";
        private CloudBlobContainer container;
        private ILogger logger;

        public Pictures(HttpContext context) {
            logger = context.RequestServices.GetRequiredService<ILogger<IWebHost>>();

            IStorageTokenCredentialProvider provider =
              new StorageEnvironmentTokenCredentialProvider();
            var tokenCred = provider.GetTokenCredentialAsync().Result;
            logger.LogInformation($"token cred: {tokenCred}");

            StorageCredentials credential = new StorageCredentials(tokenCred);

            container = new CloudBlobContainer(
              new Uri(string.Format("{0}{1}", provider.GetBlobEndpoint(), picturesContainerName)), credential);
              logger.LogInformation($"container URI: {container.Uri}");

            OperationContext storageContext = new OperationContext();
            var succeeded = container.CreateIfNotExistsAsync(
              BlobContainerPublicAccessType.Blob,
              new BlobRequestOptions(),
              storageContext,
              CancellationToken.None).Result;
            logger.LogInformation($"storage context: {storageContext.RequestResults}");
        }
        
        public async Task<String> PutPicture(string filename, System.IO.Stream filebytes) {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);
            Console.WriteLine($"going to put stream {blockBlob.Uri.ToString()}");
            await blockBlob.UploadFromStreamAsync(filebytes);
            // this URI will be stored with profile, without SAS token appended
            return blockBlob.Uri.ToString();
        }

        #pragma warning disable 1998
        public async Task<String> GetPictureUrl(string filename) {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);
            // TODO: add SAS token to returned URI
            return blockBlob.Uri.ToString();
        }
    }
}
