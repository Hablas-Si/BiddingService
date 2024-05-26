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
var RedisPW = await vaultService.GetSecretAsync("RedisPW");
var redisConnect = await vaultService.GetSecretAsync("redisConnect");

Console.WriteLine($"Secret: {mySecret} and Issuer: {myIssuer}");
Console.WriteLine($"RedisPW: {RedisPW}");
Console.WriteLine($"redisConnect: {redisConnect}");
if (mySecret == null || myIssuer == null || RedisPW == null || redisConnect == null)
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
var redisConnectionString = "redis-16675.c56.east-us.azure.redns.redis-cloud.com:16675";
var redisPassword = "0rIwX58ixdvj6btmfJrxvsxaMn3s4uta";

builder.Services.AddSingleton<RedisCacheService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<RedisCacheService>>();
    return new RedisCacheService(redisConnectionString, redisPassword, logger);
});


builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQSettings"));

builder.Services.AddSingleton<RabbitMQPublisher>(sp =>
{
    var rabbitMQSettings = new RabbitMQSettings();
    builder.Configuration.GetSection("RabbitMQSettings").Bind(rabbitMQSettings); // Bind settings from configuration

    Console.WriteLine("RabbitMQ Settings:");
    Console.WriteLine(rabbitMQSettings.Hostname);
    Console.WriteLine(rabbitMQSettings.QueueName);

    return new RabbitMQPublisher(rabbitMQSettings.Hostname, rabbitMQSettings.QueueName);
});

var auctionServiceUrl = Environment.GetEnvironmentVariable("auctionServiceUrl");
if (string.IsNullOrEmpty(userServiceUrl))
{
    logger.Error("auctionServiceUrl is missing");
    throw new ApplicationException("auctionServiceUrl is missing");
}
else
{
    logger.Info("auctionServiceUrl is: " + userServiceUrl);
}
// tilf�jer Repository til services
builder.Services.AddSingleton<IBiddingRepository, BiddingRepository>(client =>
{
    //client.BaseAddress = new Uri(auctionServiceUrl);
});


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
