using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;
using VaultSharp.V1.AuthMethods;

namespace BiddingService.Repositories
{
    // Repository class for interacting with HashiCorp Vault to retrieve secrets
    public class VaultRepository : IVaultRepository
    {
        private readonly IVaultClient _vaultClient; // Vault client for accessing Vault API
        private readonly NLog.ILogger _logger; // Logger for logging information

        // Constructor for initializing the VaultRepository with necessary configurations
        public VaultRepository(NLog.ILogger logger, IConfiguration config)
        {
            // Retrieve Vault endpoint and token from configuration
            var EndPoint = config["Address"];
            var token = config["Token"];
            _logger = logger;

            // Log the Vault endpoint and token (for debugging purposes)
            _logger.Info($"VaultService: {EndPoint} and {token}");

            // Configure HttpClientHandler to ignore SSL certificate errors
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                (message, cert, chain, sslPolicyErrors) => { return true; };

            // Set up the authentication method using the provided token
            IAuthMethodInfo authMethod = new TokenAuthMethodInfo(token);

            // Configure Vault client settings
            var vaultClientSettings = new VaultClientSettings(EndPoint, authMethod)
            {
                Namespace = "",
                MyHttpClientProviderFunc = handler => new HttpClient(httpClientHandler)
                {
                    BaseAddress = new Uri(EndPoint)
                }
            };

            // Initialize the Vault client with the configured settings
            _vaultClient = new VaultClient(vaultClientSettings);
        }

        // Method to retrieve a secret from Vault
        public async Task<string> GetSecretAsync(string path)
        {
            try
            {
                // Attempt to read the secret from Vault using the specified path and mount point
                Secret<SecretData> kv2Secret = await _vaultClient.V1.Secrets.KeyValue.V2
                    .ReadSecretAsync(path: "hemmeligheder", mountPoint: "secret");

                // Extract the secret value from the retrieved data
                var secretValue = kv2Secret.Data.Data[path];

                // Return the secret value as a string
                return secretValue.ToString();
            }
            catch (Exception ex)
            {
                // Log an error message if the secret retrieval fails
                Console.WriteLine($"Error retrieving secret: {ex.Message}");

                // Return null to indicate failure
                return null;
            }
        }
    }
}
