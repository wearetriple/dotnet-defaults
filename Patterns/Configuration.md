# Configuration

Configuration in dotnet applications in generally handled through the IConfiguration implementation, which allows multiple providers to supply key,value data to use in the application. [Keyvault](./KeyVault.md) is one such a provider. [Options](./Options.md) Shows an example how to handle the IConfiguration via dependency injection an how to setup classes to map the key value data to.

Besides Keyvault some commonly added providers are the `JsonConfigurationProvider` which is generally used to read an appsettings.json file which contains nested json data.

```csharp
var filePath = Path.Combine(applicationRootPath, "appsettings.common.json");
configurationBuilder.AddJsonFile(filePath, optional: true, reloadOnChange: false);
```

Another commonly used provider is the `EnvironmentVariablesConfigurationProvider`.

```csharp
configurationBuilder.AddEnvironmentVariables();
```

This provider has some caveats, mainly because environment variables are a flat (meaning just `key_string: value_string`) dictionary not containing a nested structure. But configuration is generally setup with nested sections. for example a logging section with a section for different log options. In an appsettings.json file this would look something like:

```json
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
```

This can also be stored in environment variables as follows:

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

As you can see both `:` or `__` works as separator.

This is also how you can add these settings to the configuration/appsettings section in a webapp or functionapp.

A similar approach is necessary when using the keyvault, but instead of `:` or `__` the keyvault requires `--` to separate the sections.

## Azure functions

**Warning** local.settings.json is not a appsettings.json.

When locally developing azure functions you are required to create a `local.settings.json`. Which minimally looks like this:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet"
  }
}
```

This file helps local developers by setting all values in the `Values` dictionary as environment variables so it is not necessary to add them to your shell. So this list is also a flat dictionary and requires the `:` separator when adding sections in the key.

The `local.settings.json` file is locally useful because it replaces the Azure function App Settings configuration.

⚠️ This file should not be used with the `JsonConfigurationProvider`

## Environment variables

Environment variables are like the name suggest variables specifically for an environment, but they mainly reference the variables that a process has access to that are set by the operating system.

```bash
# CMD
echo %AppData%
echo %Temp%
echo %Username%
echo %Path%

# Powershell
echo $env:AppData
echo $env:Temp
echo $env:Username
echo $env:Path

# Bash
echo $PATH
echo $USER
```

When starting a process from a shell(e.g. CMD, Powershell and Bash) system set environment variables are loaded as well as locally set variables with only live until you close the shell again.

Some ways of setting this locally set variables are:

```bash
# Bash
set ENABLE_NETWORKING=1
set FLASK_NAME="Python process"

# Powershell
$env:ENABLE_NETWORKING=1
$env:FLASK_NAME="Python process"

# Bash
ENABLE_NETWORKING=1
FLASK_NAME="Python process"
```

Now when you run your process from the shell (e.g. `func start` or `dotnet run` or `process.exe`) there is a way to access them.

```csharp
# Sample way of directly accessing the variable
using System;

var AppData = Environment.GetEnvironmentVariable("AppData");
```

### Adding persistent environment variable

On windows you can use the `SystemPropertiesAdvanced.exe` (under the advanced tab) and click the `Environment Variables` button at the bottom to open an interface where you can manage your environment variables.

[.NET runtime Environment variables docs](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables)
