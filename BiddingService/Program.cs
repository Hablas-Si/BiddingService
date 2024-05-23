using BiddingService.Repositories;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using BiddingService.Services;
using Microsoft.Extensions.Options;
using BiddingService.Settings;
using NLog;
using NLog.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings()
.GetCurrentClassLogger();
logger.Debug("init main");

var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();
builder.Host.UseNLog();

// BsonSeralizer... fort�ller at hver gang den ser en Guid i alle entiteter skal den serializeres til en string. 
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

// Fetch secrets from Vault. Jeg initierer vaultService og bruger metoden derinde GetSecretAsync
var vaultService = new VaultRepository(logger, builder.Configuration);
var mySecret = await vaultService.GetSecretAsync("Secret");
var myIssuer = await vaultService.GetSecretAsync("Issuer");
// logger.Info($"Secret: {mySecret} and Issuer: {myIssuer}");
if (mySecret == null || myIssuer == null)
{
    Console.WriteLine("Failed to retrieve secrets from Vault");
    throw new ApplicationException("Failed to retrieve secrets from Vault");
}
builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = myIssuer,
        ValidAudience = "http://localhost",
        IssuerSigningKey =
    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(mySecret))
    };
});
// Tilføjer authorization politikker som bliver brugt i controlleren, virker ik
builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
    });
// Add services to the container.

//Connectionstring henter den fra Vault
var ConnectionAuctionDB = await vaultService.GetSecretAsync("ConnectionAuctionDB");
builder.Services.Configure<MongoDBSettings>(options =>
{
    options.ConnectionAuctionDB = ConnectionAuctionDB ?? throw new ArgumentNullException("ConnectionAuctionDB environment variable not set");
});

//tilføjer Repository til services.
builder.Services.AddSingleton<IVaultRepository>(vaultService);

// Register RedisCacheService
builder.Services.AddSingleton<RedisCacheService>(sp =>
{
    // Define the Redis password
    string redisPassword = "0rIwX58ixdvj6btmfJrxvsxaMn3s4uta"; //OBS: HEMMELIGHED

    // Define the default database index (e.g., 0)
    int defaultDatabaseIndex = 0; //OBS: HEMMELIGHED?

    // Construct the Redis connection string with the password and default database index
    string redisConnectionString = $"redis-16675.c56.east-us.azure.redns.redis-cloud.com:16675,DefaultDatabase={defaultDatabaseIndex},password={redisPassword}"; // OBS: HEMMELIGHED
    return new RedisCacheService(redisConnectionString);
});

// Configure RabbitMQ settings OBS: BRUGER IKKE SETTINGSKLASSEN, MEN APPSETTINGS
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQSettings")); 
builder.Services.AddSingleton<RabbitMQPublisher>(sp =>
{
    var rabbitMQSettings = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
    return new RabbitMQPublisher(rabbitMQSettings.Hostname, rabbitMQSettings.QueueName);
});

// tilf�jer Repository til services
builder.Services.AddSingleton<IBiddingRepository, BiddingRepository>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


//app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();
