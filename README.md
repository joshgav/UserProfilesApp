# User Profiles App

This app uses Azure Active Directory via OpenIDConnect; Azure CosmosDB; and
Azure Blob Storage to maintain a database of user profiles. It demonstrates
several authn/z forms used in Azure and alternatives for managing secrets
needed for them.

# Description

For the app to work you'll first need to set up resources and secrets as
described in the ensuing sections. Then, enter `dotnet run` and navigate to
`https://localhost:5001/hello` to initiate the following flow:

1. User is redirected to and authenticated by Azure Active Directory via
   OpenIDConnect; which then redirects them back to the app page.
1. The app page checks the user's identity. If the user has logged in before,
   profile info for that identity is retieved from a CosmosDB collection.
1. If the user has not logged in before, a new profile is added to the CosmosDB
   collection based on info in the `id_token`.
1. An HTML form is returned which provides info about the user. Some fields can
   be modified. When the "Update Profile" button is clicked the values of these
   fields are updated in the CosmosDB database.
1. The signed-in user can supply a picture in the "picture" form element.  The
   picture is stored in Azure Blob Storage and its URL is stored with the
   user's profile in the profile database. The pictures are publicly readable
   so they can be easily rendered in the profile form.

# Setup

The following resources must be created for use by the app. Names used within
scripts can be set in [scripts/vars.sh](./scripts/vars.sh).

1. Set up a CosmosDB account and database; a collection named "profiles" will
   be created within the database by the app. See and/or use
   [scripts/setup\_cosmos.sh](./scripts/setup_cosmos.sh).
1. Set up a Storage account; a container named `pictures` will be created
   within the account by the app. See and/or use
   [scripts/setup\_storage.sh](./scripts/setup_storage.sh).
1. Set up an AAD v1.0 app and grant it "Storage Contributor" access to the
   account created above. See and/or use
   [scripts/setup\_principal.sh](./scripts/setup_principal.sh).
1. Set up an AAD v2.0 app at <https://apps.dev.microsoft.com>. Generate and
   note the app ID and secret.

# Getting and using secrets

Three Azure services and forms of authorization are used in this app:

* **Active Directory** v2.0 with OpenIDConnect.
* **CosmosDB** with shared secret key.
* **Blob Storage** with AAD v1.0 and OAuth.

Types of sources for secrets or other forms of authentication include the
following. Currently each service in this app gets secrets from the
environment; the alternatives listed [below](#alternatives) are in development.

* **Environment**: secrets are stored in environment variables,
  `appsettings.json`, `dotnet user-secrets ...`, and other locations in the
  hosting environment.
* **Key Vault**: secrets are stored statically in a vault and retrieved at
  runtime by an app.
* **Resource Manager**: secrets are retrieved from Azure RM control-plane APIs.
* **Managed Identity**: secrets are not needed to attest to an identity because
  the platform attests to it.

## Environment

The base implementation uses secrets found in the environment as follows. See
and/or use [scripts/setup\_secrets.sh](./scripts/setup_secrets.sh) for exact
details.

User authentication through **Active Directory** and OpenIDConnect requires a
Client ID and secret for AADv2, which can be gotten at
<https://apps.dev.microsoft.com>. These values should be stored in the
environment as follows:

```bash
dotnet user-secrets set 'Azure:DirectoryV2:ClientId' '<client_id>'
dotnet user-secrets set 'Azure:DirectoryV2:ClientSecret' '<client_secret>' ```
```

Access to **CosmosDB** requires an account key and endpoint, which can be
gotten with `az cosmosdb list-keys ...` and `az cosmosdb show ...`. Values
should be stored in the environment as follows:

```bash
dotnet user-secrets set 'Azure:CosmosDB:Key' '<key>' dotnet
user-secrets set 'Azure:CosmosDB:Endpoint' '<endpoint>' ```
```

Authorization for **Blob Storage** requires a Client ID and secret for AADv1;
and the client must be authorized to access the container used by this app. See
and use [scripts/setup\_principal.sh](./scripts/setup_principal.sh) for
details.  Values should be stored in the environment as follows:

```bash
dotnet user-secrets set 'Azure:DirectoryV1:ClientId' '<client_id>'
dotnet user-secrets set 'Azure:DirectoryV1:ClientSecret' '<client_secret>' ```
```

## Alternatives

We can reduce use of secrets by leveraging Managed Identity and Key Vault, in
the following ways. Implementation of these is tracked in
[#1](https://github.com/joshgav/UserProfilesApp/issues/1) and
[#2](https://github.com/joshgav/UserProfilesApp/issues/2).

* Get CosmosDB account key from Key Vault using service principal.
* Replace service principal with managed identity:
  * Get CosmosDB account key from Key Vault using managed identity.
  * Get Blob Storage token using managed identity.
* Get CosmosDB account key from Azure RM using managed identity (avoid Key
  Vault).

