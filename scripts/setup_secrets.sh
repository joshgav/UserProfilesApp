#! /usr/bin/env bash

script_dir=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)
source ${script_dir}/vars.sh

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

dotnet user-secrets set 'Azure:CosmosDB:Endpoint' "$cosmosdb_endpoint"
dotnet user-secrets set 'Azure:CosmosDB:Key' "$cosmosdb_key"

storage_account_id=$(az storage account show \
  --name $storage_account_name \
  --resource-group $group_name \
  --output tsv --query id 2> /dev/null)

storage_blob_uri=$(az storage account show \
  --ids $storage_account_id \
  --query 'primaryEndpoints.blob' --output tsv)

dotnet user-secrets set 'Azure:Storage:Blob:Endpoint' $storage_blob_uri

${script_dir}/setup_principal.sh true  # generate new password

# get from https://apps.dev.microsoft.com
# dotnet user-secrets set 'Azure:DirectoryV2:ClientId' ""
# dotnet user-secrets set 'Azure:DirectoryV2:ClientSecret' ""
