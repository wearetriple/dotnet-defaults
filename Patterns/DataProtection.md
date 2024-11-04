# Data protection in a .Net application

In the past .Net Framework used a machinekey as a key to use when encrypting tokens like the token in an authentication
cookie or the anti forgery token used in forms. 

However, with the release of .Net Core Microsoft removed the machine key and added the [Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction) libraries to 
generate a key in memory on application startup. This can cause some issues with application that are scaled or are 
relatively often restarted as this key is regenerated each time an application starts without a common seed, 
meaning it will differ between runtimes and instances. 

Most often these issues will be forced logouts or forms that can't be posted.

Recommended is to implement custom Data Protection using any of the default providers when dealing with encryption.

For an explanation how to implement data protection see this url: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview

Below is an example setup as used by the WeAreTriple website:
```csharp
public static void AddDataProtectionWithKeyVault(this IServiceCollection services,
        IConfiguration config)
    {
        var serverOptions = new ServerOptions();
        config.GetSection(ServerOptions.Server).Bind(serverOptions);

        var azureBlobOptions = new AzureBlobOptions();
        config.GetSection(AzureBlobOptions.Name).Bind(azureBlobOptions);

        if (serverOptions.Role == "replica")
        {
            services.AddDataProtection()
                .DisableAutomaticKeyGeneration();
        }

        services.AddDataProtection()
            .SetApplicationName(serverOptions.ApplicationName ?? Assembly.GetExecutingAssembly().GetName().FullName)
            .PersistKeysToAzureBlobStorage(azureBlobOptions.Keys?.ConnectionString, azureBlobOptions.Keys?.ContainerName,
                azureBlobOptions.Keys?.BlobName);
    }
```
In this application we only want a `single or publisher` server to create and persist the keys to storage while the `replica`
servers are only allowed to read the keys. With the current configuration we can share the key between instances thanks 
to the `SetApplicationName` method and use the default key rolling settings (90 days).