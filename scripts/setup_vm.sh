#! /usr/bin/env bash

script_dir=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)
source ${script_dir}/vars.sh

group_id=$(az group show --name $group_name --output tsv --query id)
if [ -z $group_id ]; then
    group_id=$(az group create --name $group_name --location $location --output tsv --query id)
fi

vm_group_id=$(az group show --name $vm_group_name --output tsv --query id)
if [ -z $vm_group_id ]; then
    vm_group_id=$(az group create --name $vm_group_name --location $location --output tsv --query id)
fi

vm_id=$(az vm show \
  --name $vm_name \
  --resource-group $vm_group_name \
  --query id --output tsv)

if [ -z $vm_id ]; then
  image_urn=Canonical:UbuntuServer:18.04-LTS:18.04.201809110
  vm_id=$(az vm create \
    --name $vm_name \
    --resource-group $vm_group_name \
    --custom-data "${script_dir}/cloud-init.yml" \
    --image $image_urn \
    --ssh-key-value "~/.ssh/id_joshgav_rsa.pub" \
    --assign-identity '[system]' \
    --role Contributor \
    --scope $vm_group_id \
    --query id --output tsv)
fi

vm_identity_id=$(az vm identity show \
  --name $vm_name \
  --resource-group $vm_group_name \
  --query 'principalId' --output tsv)

az keyvault set-policy \
  --name $keyvault_name \
  --resource-group $group_name \
  --object-id $vm_identity_id \
  --secret-permissions "get" "list"
