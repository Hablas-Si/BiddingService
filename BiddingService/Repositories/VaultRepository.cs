using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;
using VaultSharp.V1.AuthMethods;


namespace BiddingService.Repositories
{
    public class VaultRepository : IVaultRepository
    {
        private readonly IVaultClient _vaultClient;
        private readonly NLog.ILogger _logger;


        public VaultRepository(NLog.ILogger logger, IConfiguration config)
        {
            var EndPoint = config["Address"];
            var token = config["Token"];
            _logger = logger;

            _logger.Info($"VaultService: {EndPoint} and {token}");
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                (message, cert, chain, sslPolicyErrors) => { return true; };
            IAuthMethodInfo authMethod =
                new TokenAuthMethodInfo(token);
            var vaultClientSettings = new VaultClientSettings(EndPoint, authMethod)
            {
                Namespace = "",
                MyHttpClientProviderFunc = handler
                    => new HttpClient(httpClientHandler)
                    {
                        BaseAddress = new Uri(EndPoint)
                    }
            };
            _vaultClient = new VaultClient(vaultClientSettings);
        }

        public async Task<string> GetSecretAsync(string path)
        {
            try
            {

                Secret<SecretData> kv2Secret = await _vaultClient.V1.Secrets.KeyValue.V2
                    .ReadSecretAsync(path: "hemmeligheder", mountPoint: "secret");
                var secretValue = kv2Secret.Data.Data[path];
                return secretValue.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving secret: {ex.Message}");
                return null;
            }
        }
    }
}