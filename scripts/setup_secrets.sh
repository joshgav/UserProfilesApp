#! /usr/bin/env bash

script_dir=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)
source ${script_dir}/vars.sh

user_secrets=${1:-"false"}
keyvault_secrets=${2:-"false"}

cosmosdb_account_id=$(az cosmosdb show \
  --name $cosmosdb_account_name \
  --resource-group $group_name \
  --output tsv --query id)

cosmosdb_key=$(az cosmosdb list-keys \
  --ids $cosmosdb_account_id \
  --query 'primaryMasterKey' --output tsv)

cosmosdb_endpoint=$(az cosmosdb show \
  --ids $cosmosdb_account_id \
  --query 'documentEndpoint' --output tsv)

storage_account_id=$(az storage account show \
  --name $storage_account_name \
  --resource-group $group_name \
  --output tsv --query id 2> /dev/null)

storage_blob_uri=$(az storage account show \
  --ids $storage_account_id \
  --query 'primaryEndpoints.blob' --output tsv)

if [ "$user_secrets" = "user_secrets" ]; then
  dotnet user-secrets set 'Azure:CosmosDB:Endpoint' "$cosmosdb_endpoint"
  dotnet user-secrets set 'Azure:CosmosDB:Key' "$cosmosdb_key"
  dotnet user-secrets set 'Azure:Storage:Blob:Endpoint' $storage_blob_uri
fi

if [ "$keyvault_secrets" = "keyvault_secrets" ]; then
  az keyvault secret set \
    --vault-name $keyvault_name \
    --name 'Azure--CosmosDB--Endpoint' \
    --value "$cosmosdb_endpoint"
  az keyvault secret set \
    --vault-name $keyvault_name \
    --name 'Azure--CosmosDB--Key' \
    --value "$cosmosdb_key"
  az keyvault secret set \
    --vault-name $keyvault_name \
    --name 'Azure--Storage--Blob--Endpoint' \
    --value "$storage_blob_uri"
fi

${script_dir}/setup_storage_principal.sh \
  generate_new_password $user_secrets $keyvault_secrets

echo "### Keyvault Secrets ###"
az keyvault secret list \
  --vault-name $keyvault_name \
  --query '[].id' --output tsv

echo "### Local Secrets ###"
dotnet user-secrets list

# get from https://apps.dev.microsoft.com
# dotnet user-secrets set 'Azure:DirectoryV2:ClientId' ""
# dotnet user-secrets set 'Azure:DirectoryV2:ClientSecret' ""
# az keyvault secret set --vault-name $keyvault_name --name 'Azure--DirectoryV2--ClientId' --value ""
# az keyvault secret set --vault-name $keyvault_name --name 'Azure--DirectoryV2--ClientSecret' --value ""

# az keyvault secret list --vault-name $keyvault_name --query '[].id'
