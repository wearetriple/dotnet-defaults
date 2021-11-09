using System;
using System.IO;
using System.Linq;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
#if NET5_0
using Microsoft.Extensions.Hosting;
#elif NETCOREAPP3_1
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
#endif


namespace Triple.KeyVault
{
    public static class KeyVaultConfigurationHelper
    {
#if NET5_0
        /// <summary>
        /// Adds KeyVault configuration
        /// 
        /// dotnet 5.0 style
        /// </summary>
        /// <param name="builder"></param>
        public static void AddKeyVault(HostBuilderContext context, IConfigurationBuilder configurationBuilder)
        {
            AddKeyVault(configurationBuilder, context.HostingEnvironment.ContentRootPath);
        }
#elif NETCOREAPP3_1
        /// <summary>
        /// Adds KeyVault configuration
        /// 
        /// dotnet core 3.1 style
        /// </summary>
        /// <param name="builder"></param>
        public static void AddKeyVault(IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();
            AddKeyVault(builder.ConfigurationBuilder, context.ApplicationRootPath);
        }
#endif

        private static void AddKeyVault(IConfigurationBuilder builder, string rootPath)
        {
            // ConfigurationBuilder already contains EnvironmentVariableProvider but we expect it to
            // have the highest priority so we remove it and add it again after the appsettings.json provider
            ConfigurationRemoveEnvironmentVariables(builder);

            builder
                .AddJsonFile(Path.Combine(rootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var currentConfig = builder.Build();
            var keyVaultName = currentConfig.GetValue<string>("KeyVaultName");
            var keyVaultPrefix = currentConfig.GetValue<string>("KeyVaultPrefix");
            var keyVaultRefreshInterval = currentConfig.GetValue<TimeSpan?>("KeyVaultInterval");

            if (!string.IsNullOrWhiteSpace(keyVaultName))
            {
                try
                {
                    var secretClient = CreateSecretClient(keyVaultName);

                    builder.AddAzureKeyVault(
                        secretClient,
                        new AzureKeyVaultConfigurationOptions
                        {
                            Manager = string.IsNullOrWhiteSpace(keyVaultPrefix)
                                ? new KeyVaultSecretManager()
                                : new PrefixKeyVaultSecretManager(keyVaultPrefix),
                            ReloadInterval = keyVaultRefreshInterval
                        });
                }
                catch
                {
                    // this exception is swallowed because there is no way of logging this exception, 
                    // and crashing here will stop the startup of the application
                    return;
                }
            }
        }

        private static void ConfigurationRemoveEnvironmentVariables(IConfigurationBuilder configurationBuilder)
        {
            var existingEnvConfigurationSource = configurationBuilder.Sources
                .FirstOrDefault(x => x is EnvironmentVariablesConfigurationSource);

            if (existingEnvConfigurationSource != null)
            {
                configurationBuilder.Sources.Remove(existingEnvConfigurationSource);
            }
        }

        private static SecretClient CreateSecretClient(string? keyVaultName)
        {
            var secretClient = new SecretClient(
                new Uri($"https://{keyVaultName}.vault.azure.net/"),
                new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    // force using AD tenant to prevent external profiles from getting tokens from incorrect ADs
                    InteractiveBrowserTenantId = "{tenant-id}",
                    SharedTokenCacheTenantId = "{tenant-id}",
                    VisualStudioCodeTenantId = "{tenant-id}",
                    VisualStudioTenantId = "{tenant-id}"
                })
            );

            return secretClient;
        }
    }
}
