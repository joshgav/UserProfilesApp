A server which authenticates the user via OAuth2, then stashes profile info in a database and a storage container.
Demonstrates user authentication with AADv2, CosmosDB access with shared secrets, and Azure Storage access with AADv1.

1. Replace saved credentials for Azure Storage with managed identity.
2. Replace saved credentials for CosmosDB with Key Vault secrets.
3. Replace saved credentials for CosmosDB with dynamic secrets via managed identity with high permissions.
