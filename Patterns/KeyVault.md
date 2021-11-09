# KeyVault

[Azure KeyVault](https://azure.microsoft.com/en-us/services/key-vault/#product-overview) allows for storing secrets and certificates which can be accessed programmatically. KeyVault solves the issue of having to share or save secrets in a repository, while keeping the option to configure specific developers to have specific access rights.

Each environment (Dev, Test, Acc and Prod) should have its own KeyVault. The KeyVaults of environments that are deployed automatically (Test, Acc and Prod) should be included in the deployment template of that environment, and services that require access to the KeyVault (App Services, VMs, etc) should access the KeyVault via their Managed Identity. The Managed Identity should be given a specific Access Policy in the KeyVault (usually LIST and GET) and that policy should be bound to the Principal Id of the Managed Identity.

The development environment should also have its own KeyVault, which can be created manually in the Azure Portal. To give a user access to the KeyVault an Access Policy should be created for that specific user, with at least LIST and GET permissions. 

## How to implement KeyVault

Since the KeyVault is another source of configuration, the setup of KeyVault as config provider must happen at the start of the application. A reference implementation is included in [NETDefault/Libs/Triple.KeyVault](https://github.com/wearetriple/dotnet-defaults/tree/keyvault/NETDefault/Libs/Triple.KeyVault). This consists of a helper which must be invoked at configuration initialization and a secret manager that supports prefixes.

### HostBuilder based applications

Applications based on `HostBuilder` (ASP.NET Core, Azure Functions .NET 5) should setup the KeyVault provider by calling `ConfigureAppConfiguration` on the `HostBuilder`.

### Azure Functions .NET Core 3.1

Azure Functions running on .NET Core 3.1 should have a [Function Startup](https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection) and override the method `ConfigureAppConfiguration`. 

### PrefixKeyVaultSecretManager

When multiple applications (imagine CMS, API and background services) use the same KeyVault, naming collisions can occur since. The PrefixKeyVaultSecretManager accepts a prefix (like `CMS--`) and strips that prefix from all the secrets it reads from the KeyVault. This way, the secrets `CMS--Cache--ConnectionString` and `API--Cache--ConnectionString` will both be read to the section `Cache` and property `ConnectionString`, but can have different values for the CMS and API application.

## Azure AD confusion and token issues

The KeyVault clients all rely on an instance of `DefaultAzureCredential` which resolves an access token that is used to access the KeyVault. In Azure, the Managed Identity is used and since this Managed Identity originates from the same directory as where the KeyVault is deployed, so there is no risk in getting the Azure AD mixed up.

When a developer want to access the KeyVault, a mixup can occur when the Azure AD of the developer is different than the Azure AD of the KeyVault. The `DefaultAzureCredential` developer will resolve an access token created the AD of the developer, which will not be accepted by the KeyVault in the other AD. To fix this, the `DefaultAzureCredential` should be given a `DefaultAzureCredentialOptions` which contains the pinned tenant ids of the AD of the KeyVault.

Next to the pinned ids, the developer should also be signed in into Visual Studio with the account that has access to the KeyVault, or have installed Azure CLI, ran `az login` and logged in with the account that has access.

## Startup failures

Please mind that if the secret client cannot access the KeyVault the application fails to start. One can implement a check prior to adding the provider to test whether the KeyVault connection is okay, or make sure the connection is always sound.
