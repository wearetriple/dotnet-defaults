# KeyVault

[Azure KeyVault](https://azure.microsoft.com/en-us/services/key-vault/#product-overview) allows for storing secrets and certificates which can be accessed programmatically. KeyVault solves the issue of having to share or save secrets in a repository, while keeping the option to configure specific developers to have specific access rights.

Each environment (Dev, Test, Acc and Prod) should have its own KeyVault. The KeyVaults of environments that are deployed automatically (Test, Acc and Prod) should be included in the deployment template of that environment, and services that require access to the KeyVault (App Services, VMs, etc) should access the KeyVault via their Managed Identity. The Managed Identity should be given a specific Access Policy in the KeyVault (usually LIST and GET) and that policy should be bound to the Principal Id of the Managed Identity.

The development environment should also have its own KeyVault, which can be created manually in the Azure Portal. To give a user access to the KeyVault an Access Policy should be created for that specific user, with at least LIST and GET permissions. 

## Azure AD confusion and token issues

The KeyVault clients all rely on an instance of `DefaultAzureCredential` which resolves an access token that is used to access the KeyVault. In Azure, the Managed Identity is used and since this Managed Identity originates from the same directory as where the KeyVault is deployed, so there is no risk in getting the Azure AD mixed up.

When a developer want to access the KeyVault, a mixup can occur when the Azure AD of the developer is different than the Azure AD of the KeyVault. The `DefaultAzureCredential` developer will resolve an access token created the AD of the developer, which will not be accepted by the KeyVault in the other AD. To fix this, the `DefaultAzureCredential` should be given a `DefaultAzureCredentialOptions` which contains the pinned tenant ids of the AD of the KeyVault.

Next to the pinned ids, the developer should also be signed in into Visual Studio with the account that has access to the KeyVault, or have installed Azure CLI, ran `az login` and logged in with the account that has access.

## Startup failures

Please mind that if the secret client cannot access the KeyVault the application fails to start. One can implement a check prior to adding the provider to test whether the KeyVault connection is okay, or make sure the connection is always sound.
