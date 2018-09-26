#! /usr/bin/env bash

script_dir=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)
source ${script_dir}/vars.sh
generate_new_password=${1:-"false"}

group_id=$(az group show --name $group_name --output tsv --query id)
if [ -z $group_id ]; then
    group_id=$(az group create --name $group_name --location $location --output tsv --query id)
fi

app_id=$(az ad app show \
  --id $directory_app_identifier \
  --query 'appId' --output tsv)

if [ -z $app_id ]; then
  app_id=$(az ad app create \
    --display-name $directory_app_name \
    --identifier-uris $directory_app_identifier \
    --key-type Symmetric \
    --key-usage Sign \
    --reply-urls 'https://localhost:5001/signin' \
    --query 'appId' --output tsv)
fi

sp_id=$(az ad sp show \
  --id $directory_app_identifier \
  --query 'appId' --output tsv)

if [ -z $sp_id ]; then
  sp_id=$(az ad sp create \
    --id $directory_app_identifier \
    --query 'appId' --output tsv)
fi

if [ "$generate_new_password" = "true" ]; then
  new_password=$(uuidgen)
  az ad app update \
    --id $directory_app_identifier \
    --password $new_password
  echo "Password: $new_password"
  dotnet user-secrets set 'Azure:DirectoryV1:ClientId' "$sp_id"
  dotnet user-secrets set 'Azure:DirectoryV1:ClientSecret' "$new_password"
fi

# Link: https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#storage-blob-data-contributor-preview
storage_role_name="Storage Blob Data Contributor (Preview)"
storage_role_id="ba92f5b4-2d11-453d-a403-e96b0029c9fe"
role_assignment_id=$(az role assignment list \
  --role $storage_role_id \
  --assignee $sp_id \
  --scope $group_id \
  --query '[0].id' --output tsv)

if [ -z $role_assignment_id ]; then
  role_assignment_id=$(az role assignment create \
    --role $storage_role_id \
    --assignee $sp_id \
    --scope $group_id \
    --query id --output tsv)
fi

echo "App ID: $app_id"
echo "Service Principal ID: $sp_id"
echo "Role Assignment ID: $role_assignment_id"
