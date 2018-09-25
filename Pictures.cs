using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ProfilesApp
{
    public class Pictures
    {
				private CloudBlobContainer container;
				private String picturesContainerName = "pictures";

        public Pictures() {
            IStorageTokenCredentialProvider provider = new StorageEnvironmentTokenCredentialProvider();
            StorageCredentials credential = new StorageCredentials(provider.GetTokenCredential());

						var blobs = new CloudBlobClient(provider.GetBlobEndpoint(), credential);
            // ensure container
						container = blobs.GetContainerReference(picturesContainerName);
        }
        
        public async Task<Uri> PutPicture([]byte fileStream) {
						string filename = "";
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            await blockBlob.UploadFromStreamAsync(fileStream);
            return await Task.FromResult(true);
        }

        public async Task<Uri> GetPictureUrl(string profileId) {

        }
    }
}
