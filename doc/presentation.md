# SDK Authentication and Secrets

Database passwords, cryptographic keys, API tokens: these are some examples
of keys and secrets that modern apps need in order to work and interact with
external resources. Every developer understands that proper management of
such secrets is important (or really bad things could happen), but choosing
and implementing good practices for secret management is hard. Azure SDKs for
C#, Java, Node.js, Python, and Go provide secure and simple ways to manage
your keys and secrets, making development of safe, modern apps easier and
faster. Come check out how!

# Agenda

* Directory, Identity, Authentication, Authorization
* Authentication and Secrets
* Secret zero and other secrets
* Authentcation and secrets in SDKs and apps

# Directory, Identity, Authentication, Authorization

* A directory contains many identities and methods of authenticating them.
* Authentication proves an identity is owned by its user.
* Authorization grants/denies rights and applies policies to an identity.

* AAD is the central directory for identities in Azure.
* AAD is also an authentication server for OAuth and OpenIDConnect.
* Services make authorization decisions based on identities in tokens.
    * Directory may include claims representing these policies.

* Show AAD in Portal: Users, Apps, Roles

## Others

Some Azure services replace these with their own authorization or directory
system.

* Resource Manager provides its own authorization policies.
    * Show a RM subscription and IAM pane. Click "Roles" and contrast with AAD roles.
    * Show CLI command to assign a role to a principal.
* Key Vault provides its own access (authorization) policies.
    * Show a Key Vault resource and its Access Policies blade.
    * Show CLI command to assign a KeyVault policy to a principal.
* Dataplane services provide their own directories:
    * CosmosDB accounts
    * Storage accounts
    * Service Bus namespaces
    * Cognitive Services accounts

# Authentication and Secrets

* Secrets prove right to identity; but platform can also attest to identity with Managed Identity.
* Managed Identity assigns an identity to a resource; that identity is granted rights.
    * Show `curl ...` of metadata endpoint in a VM.
* Limitations of managed identity:
    * Does not support authorization code flow.
    * Does not support dataplane accounts.

# Secret zero and other secrets

* Secret zero is the very first secret used to prove the app's startup identity. You can:
    1. Store it yourself and retrieve it from the environment, in e.g. appsettings.json, `dotnet user-secrets set`, environment variables.
    2. Use tokens cached in the environment by other applications like az CLI, e.g. `az account get-access-token ...`.
    3. Use Azure *managed identity* and adopt the identity attested by the environment.
* Avoid storing other secrets by allowing the identity authenticated with secret zero ("identity zero") to access them. To bootstrap from secret zero to other secrets:
    1. with *Key Vault*, create secrets statically and store them securely in a vault. Grant identity zero access to the vault.
    2. Get secrets dynamically from the control plane, i.e. Azure Resource Manager. Grant identity zero access to these operations.
* Client-side apps don't have a secure way to store secret zero. Instead:
    1. Use OAuth implicit flow.
    2. If a secret must be distributed, keeps its permissions constrained.

# Authentication and Secrets in SDKs and Apps

## Secret Zero

* From defined environment variables and files.
* From managed identity.
* From CLI login tokens.
* Authorizer chains try each of these methods so that the same codebase works with env vars and CLI tokens for local and CI tests; and with managed identity in Azure systems.

## Other secrets

* Get from Key Vault using identity zero; grant identity zero rights to the vault.
* Get dynamically from control plane using identity zero.
* Neither of these is an option for mobile since the user is unlikely to have rights to the control plane or Vault.

## Demo

Available from https://github.com/joshgav/UserProfilesApp.

* User signs in to web app using OpenIDConnect (AADv2).
* User's profile data is stored in a CosmosDB database (dataplane account and key).
* User's profile picture is stored in Azure Blob Storage (AADv1/OAuth).
* Replace multiple secrets with single "secret zero" for accessing Key Vault.
* Replace secret zero with managed identity in an Azure VM.
* Replace Key Vault secrets with managed identity:
    * get CosmosDB account/key from Azure Resource Manager
    * get Storage access tokens with AADv1/OAuth
    * but cannot replace OpenIDConnect secrets because managed identity does not support authz code

# Summary: Authn/z in Azure

## Authn/z protocols in Azure

* AADv1 - AzureRM, Storage
* AADv2 and OpenIDConnect - Microsoft Graph, Office 365
* Shared secret (API key) - Cognitive, CosmosDB, Messaging

## Authn/z supporting services in Azure

* Key Vault: store shared secrets. "Secret zero" problem persists though.
* Managed Identity: Cloud-provided principals for use in production systems without secrets. Solves "secret zero" problem.
* Combine several forms with authorizer chains (in development).

# Resources

* User Profiles App: https://github.com/joshgav/UserProfilesApp
* AAD v2.0 overview: https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-overview
* AAD metadata: https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration
* Managed identities: https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/
* Key Vault: https://docs.microsoft.com/azure/key-vault/
* Elevate AAD admins to ARM admins: https://docs.microsoft.com/en-us/azure/role-based-access-control/elevate-access-global-admin 
* Storage with AAD: https://docs.microsoft.com/rest/api/storageservices/authenticate-with-azure-active-directory
