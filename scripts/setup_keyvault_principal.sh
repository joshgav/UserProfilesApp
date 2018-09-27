#! /usr/bin/env bash

script_dir=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)
source ${script_dir}/vars.sh

generate_new_password=${1:-"false"}
user_secrets=${2:-"user_secrets"}
keyvault_secrets=${3:-"false"}

group_id=$(az group show --name $group_name --output tsv --query id)
if [ -z $group_id ]; then
    group_id=$(az group create --name $group_name --location $location --output tsv --query id)
fi

app_id=$(az ad app show \
  --id $keyvault_app_identifier \
  --query 'appId' --output tsv)

if [ -z $app_id ]; then
  app_id=$(az ad app create \
    --display-name $keyvault_principal_name \
    --identifier-uris $keyvault_app_identifier \
    --key-type Symmetric \
    --key-usage Sign \
    --reply-urls 'https://localhost:5001/signin' \
    --query 'appId' --output tsv)
fi

sp_id=$(az ad sp show \
  --id $keyvault_app_identifier \
  --query 'appId' --output tsv)

if [ -z $sp_id ]; then
  sp_id=$(az ad sp create \
    --id $keyvault_app_identifier \
    --query 'appId' --output tsv)
fi

if [ "$generate_new_password" = "generate_new_password" ]; then
  keyvault_endpoint=$(az keyvault show \
    --name $keyvault_name \
    --resource-group $group_name \
    --query 'properties.vaultUri' --output tsv)

  new_password=$(uuidgen)
  az ad app update \
    --id $keyvault_app_identifier \
    --password $new_password
  echo "App Secret: $new_password"

  if [ "$user_secrets" = "user_secrets" ]; then
    dotnet user-secrets set 'Azure:KeyVaultAccessor:ClientId' "$sp_id"
    dotnet user-secrets set 'Azure:KeyVaultAccessor:ClientSecret' "$new_password"
    dotnet user-secrets set 'Azure:KeyVaultAccessor:Endpoint' "$keyvault_endpoint"
  fi
  if [ "$keyvault_secrets" = "keyvault_secrets" ]; then
    az keyvault secret set \
      --vault-name $keyvault_name \
      --name 'Azure--KeyVaultAccessor--ClientId' \
      --value "$sp_id"
    az keyvault secret set \
      --vault-name $keyvault_name \
      --name 'Azure--KeyVaultAccessor--ClientSecret' \
      --value "$new_password"
    az keyvault secret set \
      --vault-name $keyvault_name \
      --name 'Azure--KeyVaultAccessor--Endpoint' \
      --value "$keyvault_endpoint"
  fi
fi

az keyvault set-policy \
  --name $keyvault_name \
  --resource-group $group_name \
  --spn $keyvault_app_identifier \
  --secret-permissions "get" "list" \
  2>&1 1> /dev/null

echo "App ID: $app_id"
