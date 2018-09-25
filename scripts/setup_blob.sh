#! /usr/bin/env bash

script_dir=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)
source ${script_dir}/vars.sh

group_id=$(az group show --name $group_name --output tsv --query id)
if [ -z $group_id ]; then
    az group create --name $group_name --location $location --output tsv --query id
fi

storage_account_id=$(az storage account show \
  --name $storage_account_name \
  --resource-group $group_name \
  --output tsv --query id 2> /dev/null)

if [ -z "$storage_account_id" ]; then
  storage_account_id=$(az storage account create \
    --name $storage_account_name \
    --resource-group $group_name \
    --kind Storage \
    --location $location \
    --sku Standard_RAGRS \
    --output tsv --query id)
fi

storage_blob_uri=$(az storage account show \
	--ids $storage_account_id \
	--query 'primaryEndpoints.blob' --output tsv)

# echo "Storage URI: [${storage_blob_uri}]"
# dotnet user-secrets set 'Azure:Storage:Blob:Endpoint' $storage_blob_uri
