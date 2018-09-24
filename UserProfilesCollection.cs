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
    public class UserProfilesCollection
		{
        private const string cosmosdb_db_name = "profilesapp";
        private const string cosmosdb_coll_name = "profiles";
        private DocumentClient _client;

        public UserProfilesCollection() {
            ICosmosDBKeyProvider keyProvider = new CosmosDBEnvironmentKeyProvider();

            if (_client == null) {
                 _client = new DocumentClient(
                    new Uri(keyProvider.GetEndpoint()),
                    keyProvider.GetAccountKey());
            }
						this.CreateCollectionIfNotExistsAsync().Wait();
        }

			  public async Task CreateCollectionIfNotExistsAsync()
        {
            DocumentCollection collection = new DocumentCollection();

            collection.Id = cosmosdb_coll_name;
            collection.PartitionKey.Paths.Add("/id");

            await this._client.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(cosmosdb_db_name),
                collection,
                new RequestOptions { OfferThroughput = 10000 });
        }

        public async Task<UserProfile> GetProfileAsync(String profileId)
        {
            Uri documentUri = UriFactory.CreateDocumentUri(
							cosmosdb_db_name, cosmosdb_coll_name, profileId);

            return await _client.ReadDocumentAsync<UserProfile>(
                documentUri, 
                new RequestOptions { PartitionKey = new PartitionKey(profileId) });
        }

        public async Task AddProfileAsync(UserProfile profile)
        {
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri(
							cosmosdb_db_name, cosmosdb_coll_name);
            await _client.UpsertDocumentAsync(collectionUri, profile);
        }

        public async Task RemoveProfileAsync(String profileId)
        {
            Uri documentUri = UriFactory.CreateDocumentUri(
							cosmosdb_db_name, cosmosdb_coll_name, profileId);

            await _client.DeleteDocumentAsync(
                documentUri,
                new RequestOptions { PartitionKey = new PartitionKey(profileId) });
        }

        public async Task UpdateProfileAsync(UserProfile updatedProfile)
        {
            Uri documentUri = UriFactory.CreateDocumentUri(
							cosmosdb_db_name, cosmosdb_coll_name, updatedProfile.Id);

            UserProfile originalProfile = await _client.ReadDocumentAsync<UserProfile>(
                documentUri,
                new RequestOptions { PartitionKey =
									new PartitionKey(updatedProfile.Id) });

            AccessCondition condition = new AccessCondition {
							Condition = originalProfile.Etag, Type = AccessConditionType.IfMatch };
            await _client.ReplaceDocumentAsync(
							documentUri,
              updatedProfile,
							new RequestOptions { AccessCondition = condition });
        }
    }

    public class UserProfile {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("_etag")]
        public String Etag { get; set; }

        [JsonProperty("full_name")]
        public String FullName { get; set; }

        [JsonProperty("email")]
        public String Email { get; set; }

        [JsonProperty("external_identifier")]
        public String ExternalIdentifier { get; set; }
    }
}
