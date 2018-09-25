#! /usr/bin/env bash

script_dir=$(cd $(dirname "${BASH_SOURCE[0]}") && pwd)
source ${script_dir}/vars.sh

group_id=$(az group show --name $group_name --output tsv --query id)
if [ -z $group_id ]; then
    az group create --name $group_name --location $location --output tsv --query id
fi

cosmosdb_account_id=$(az cosmosdb show \
  --name $cosmosdb_account_name \
  --resource-group $group_name \
  --output tsv --query id)

if [ -z $cosmosdb_account_id ]; then
  cosmosdb_account_id=$(az cosmosdb create \
    --name $cosmosdb_account_name \
    --resource-group $group_name \
    --default-consistency-level Strong \
    --kind GlobalDocumentDB \
    --output tsv --query id)
fi

cosmosdb_db_id=$(az cosmosdb database show \
  --db-name $cosmosdb_db_name \
  --name $cosmosdb_account_name \
  --resource-group-name $group_name \
  --output tsv --query id 2> /dev/null )

if [ -z $cosmosdb_db_id ]; then
  cosmosdb_db_id=$(az cosmosdb database create \
    --db-name $cosmosdb_db_name \
    --name $cosmosdb_account_name \
    --resource-group-name $group_name \
    --output tsv --query id)
fi

# // collection is created in app
# cosmosdb_collection_id=$(az cosmosdb collection show \
#   --collection-name $cosmosdb_collection_name \
#   --db-name $cosmosdb_db_name \
#   --name $cosmosdb_account_name \
#   --resource-group-name $group_name \
#   --output tsv --query "collection.id" 2> /dev/null )
# 
# if [ -z $cosmosdb_collection_id ]; then
#     cosmosdb_collection_id=$(az cosmosdb collection create \
#       --collection-name $cosmosdb_collection_name \
#       --db-name $cosmosdb_db_name \
#       --name $cosmosdb_account_name \
#       --resource-group-name $group_name \
#       --output tsv --query "collection.id")
# fi

cosmosdb_key=$(az cosmosdb list-keys \
  --ids $cosmosdb_account_id \
  --query 'primaryMasterKey' --output tsv)

cosmosdb_endpoint=$(az cosmosdb show \
  --ids $cosmosdb_account_id \
  --query 'documentEndpoint' --output tsv)

echo "Endpoint: [${cosmosdb_endpoint}]"
echo "Key: [${cosmosdb_key}]"
