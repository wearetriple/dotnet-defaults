using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace Triple.KeyVault
{
    /// <summary>
    /// source: https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-5.0#use-a-key-name-prefix
    /// </summary>
    public class PrefixKeyVaultSecretManager : KeyVaultSecretManager
    {
        private readonly string _prefix;

        public PrefixKeyVaultSecretManager(string prefix)
        {
            _prefix = prefix;
        }

        public override bool Load(SecretProperties secret)
        {
            return secret.Name.StartsWith(_prefix);
        }

        public override string GetKey(KeyVaultSecret secret)
        {
            return secret.Name
                .Substring(_prefix.Length)
                .Replace("--", ConfigurationPath.KeyDelimiter);
        }
    }
}
