# Configuration

Configuration in dotnet applications in generally handled through the `IConfiguration` implementation, 
which allows multiple providers to supply key,value data to use in the application. [KeyVault](./KeyVault.md) 
is one such a provider, next to JSON and environment variable configuration providers.

Using configuration throughout the application is done using the [Options pattern](./Options.md).

## Bootstrap problem

The challenge with configuration, and especially with the more advanced providers like KeyVault, is that
configuration is needed before it has been set up, making it a bit awkward to get right. To solve
this, first build a small configuration provider that reads JSON and or environment variables, and use that
provider to build the configuration provider for the application.

```csharp
var host = new HostBuilder()
    .ConfigureAppConfiguration((context, builder) => {
        var env = context.HostingEnvironment.EnvironmentName;

        // configure the initial builder 
        var initialBuilder = new ConfigurationBuilder();
        initialBuilder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables();
        
        var initialConfig = initialBuilder.Build();

        // configure the actual builder using config from the initial builder
        var credential = CredentialHelper.GetCredential(initialConfig);

        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: false);
            .AddKeyVault(credential);

        // add environment variables as last so they always override any other provider
        builder.AddEnvironmentVariables();
    })
    [..]
    // when this method is called the configuration providers are build, validated, and will construct their data
    .Build();
```

Be aware of the defaults applied to the configuration builder, sometimes some configuration is already added
in an order that's undesirable. In that case, use `builder.Remove[..]Source()` to remove a configuration provider, or
reapply a provider again so it's always at the desired position. Generally we use this order:

1. Base configuration - e.g `appsettings.json`.
2. Environment specific base configuration - e.g `application.prod.json`.
3. Environment specific external configuration - e.g KeyVault.
4. Resource specific configuration - e.g. Environment Variables.

This way base configuration can be overwritten by environment specific configuration, which in turn can
be overwritten by resource specific configuration. By allowing environment variables to overwrite any other
config, some issues can be fixed or debugged by temporarily by adding an environment variable by hand, which 
can come in handy when in a pickle. 

## Configuration providers

### Azure KeyVault

Using KeyVault for secrets (and feature flags / switches / config in small quantities) is highly advised, as it moves
secrets into a secure place. Using a KeyVault for secrets during local development can be very practical, as no exchange
of secrets between developers is required during onboarding. 

```csharp
var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    // set the tenant id (or use AZURE_TENANT_ID) so the credential provider does not get confused when using external Azure AD accounts
    TenantId = [..],
    
    // exclude all the credential providers that don't make sense for the project - this can improve startup performance while developing
    Exclude[..]Credential = true
});

// keyVaultName is typically fetched from the initial config
var secretClient = new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net/"), credential);

// use the default secret manager, or use a derivative to change its behavior (like filtering secrets based on prefix)
var keyVaultSecretManager = new KeyVaultSecretManager();

configurationBuilder.AddAzureKeyVault(
    secretClient,
    new AzureKeyVaultConfigurationOptions
    {
        Manager = keyVaultSecretManager,

        // configuring the reload interval will allow this provider to refresh periodically
        // this allows for updating / rotating secrets without application restarts
        ReloadInterval = config.KeyVaultRefreshInterval
    });
```

Multiple KeyVaults are allowed to be used in a single application, so common secrets can be configured in a common
KeyVault, while application specific secrets can be configured in a separate KeyVault. 
Use [Vaultr](https://github.com/ThomasBleijendaal/Vaultr) to manage secrets in multiple KeyVaults more easily.

### Azure App Configuration

Avoid Azure App Configuration, because:

- The service is very expensive, costing at least $36 per month for what's a simple JSON API.
- When request quota is exhausted, the service will return 429s, which will prevent new instances
of your application to start. Avoiding this quota requires spending $288 per month.
- Background refresh is unreliable and hard to trigger consistently.
- Giving customers access to this configuration still requires Azure Portal access.
- Feature flags are implemented in a clunky way, which makes them hard to use.
- The built-in CICD tooling overwrite custom config, resetting any runtime customization during deploy.

### Command line switches

If the application accepts command line switches provider via the command line arguments, use the command line
configuration provider to parse and provide these switches.

```csharp
var switchMap = new Dictionary<string, string>
{
    ["-s"] = "Options__Source",
    ["--source"] = "Options__Source",
};

configurationBuilder.AddCommandLine(args, switchMap);
```

Identical switches overwrite each other, and only primitive values are supported.

### JSON

```csharp
var filePath = Path.Combine(applicationRootPath, "appsettings.json");
configurationBuilder.AddJsonFile(filePath, optional: false, reloadOnChange: false);
```

Example config file (nested structure):

```json
{
    "Serilog": {
        "SeqServerUrl": null,
        "MinimumLevel": {
            "Default": "Information",
            "Override": {
                "Microsoft": "Warning",
                "Microsoft.eShopOnContainers": "Information",
                "System": "Warning"
            }
        }
    }
}
```

Flat structure:

```json
{
    "Serilog:SeqServerUrl": null,
    "Serilog:MinimumLevel:Default": "Information",
    "Serilog:MinimumLevel:Override:Microsoft": "Warning",
    "Serilog:MinimumLevel:Override:Microsoft.eShopOnContainers": "Information",
    "Serilog:MinimumLevel:Override:System": "Warning"
}
```

or

```json
{
    "Serilog__SeqServerUrl": null,
    "Serilog__MinimumLevel__Default": "Information",
    "Serilog__MinimumLevel__Override__Microsoft": "Warning",
    "Serilog__MinimumLevel__Override__Microsoft.eShopOnContainers": "Information",
    "Serilog__MinimumLevel__Override__System": "Warning"
}
```

### Environment variables

Environment variables are primarily used in Azure, where application configuration in services like
App Services or Container Apps is made available for your application via environment variables.

```csharp
configurationBuilder.AddEnvironmentVariables();
```

Environment variables are always flat, and must use the dunders for nested variables 
(e.g `"Serilog__MinimumLevel__Default": "Information"`).

#### Azure functions

**Warning** local.settings.json is not an appsettings.json.

When locally developing azure functions you are required to create a `local.settings.json`. 
Which minimally looks like this:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet"
  }
}
```

This file helps local developers by setting all values in the `Values` dictionary as environment variables.
To read these variables, `AddEnvironmentVariables()` must be used. `Values` is a flat dictionary 
and should use `__` as separator when adding sections in the key.
