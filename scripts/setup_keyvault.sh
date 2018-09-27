#! /usr/bin/env bash

script_dir=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)
source ${script_dir}/vars.sh

group_id=$(az group show --name $group_name --output tsv --query id)
if [ -z $group_id ]; then
    group_id=$(az group create --name $group_name --location $location --output tsv --query id)
fi

keyvault_id=$(az keyvault show \
  --name $keyvault_name \
  --resource-group $group_name \
  --query id --output tsv)

if [ -z $keyvault_id ]; then
  keyvault_id=$(az keyvault create \
    --name $keyvault_name \
    --resource-group $group_name \
    --no-self-perms "false" \
    --query id --output tsv)
fi

keyvault_endpoint=$(az keyvault show \
  --name $keyvault_name \
  --resource-group $group_name \
  --query 'properties.vaultUri' --output tsv)

current_upn=$(az account show --query 'user.name' --output tsv)
az keyvault set-policy \
  --name $keyvault_name \
  --resource-group $group_name \
  --upn $current_upn \
  --secret-permissions "backup" "delete" "get" "list" "purge" "recover" "restore" "set" \
  2>&1 1> /dev/null

${script_dir}/setup_keyvault_principal.sh \
  generate_new_password user_secrets keyvault_secrets
