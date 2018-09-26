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
				private String picturesContainerName = "pictures";
				private CloudBlobContainer container;

        public Pictures() {
            IStorageTokenCredentialProvider provider = new StorageEnvironmentTokenCredentialProvider();
            StorageCredentials credential = new StorageCredentials(provider.GetTokenCredentialAsync().Result);
            container = new CloudBlobContainer(new Uri(string.Format("{0}/{1}", provider.GetBlobEndpoint(), picturesContainerName)), credential);
        }
        
        public async Task<String> PutPicture(string filename, System.IO.Stream filebytes) {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);
            await blockBlob.UploadFromStreamAsync(filebytes);
            // this URI will be stored with profile, without SAS token appended
            return await Task.FromResult(blockBlob.Uri.ToString());
        }

        public async Task<String> GetPictureUrl(string filename) {
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(filename);
            // TODO: add SAS token to returned URI
            return await Task.FromResult(blockBlob.Uri.ToString());
        }
    }
}
