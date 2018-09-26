using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace ProfilesApp
{
    public interface ICosmosDBKeyProvider {
        String GetAccountKey();
        String GetEndpoint();
    }

    public class CosmosDBEnvironmentKeyProvider : ICosmosDBKeyProvider 
    {
        private string cosmosdb_key;
        private string cosmosdb_endpoint;

        public CosmosDBEnvironmentKeyProvider() {
        // `dotnet user-secrets set 'Azure:CosmosDB:Key' <>`
        // `dotnet user-secrets set 'Azure:CosmosDB:Endpoint' <>`
            cosmosdb_key = ProfilesApp.Configuration.Get()["Azure:CosmosDB:Key"];
            cosmosdb_endpoint =
                ProfilesApp.Configuration.Get()["Azure:CosmosDB:Endpoint"];
        }

        String ICosmosDBKeyProvider.GetAccountKey() {
            return this.GetAccountKey();
        }
        public String GetAccountKey() {
            return cosmosdb_key;
        }

        String ICosmosDBKeyProvider.GetEndpoint() {
            return this.GetEndpoint();
        }
        public String GetEndpoint() {
            return cosmosdb_endpoint;
        }
    }
}
