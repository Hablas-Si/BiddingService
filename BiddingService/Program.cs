using BiddingService.Repositories;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using BiddingService.Services;
using Microsoft.Extensions.Options;
using BiddingService.Settings;

var builder = WebApplication.CreateBuilder(args);

// BsonSeralizer... fortæller at hver gang den ser en Guid i alle entiteter skal den serializeres til en string. 
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

// OBS: lig dem her op i vault, se opgave
string mySecret = Environment.GetEnvironmentVariable("Secret") ?? "none";
string myIssuer = Environment.GetEnvironmentVariable("Issuer") ?? "none";

builder.Services.Configure<MongoDBSettings>(options =>
{
    //options.ConnectionURI = Environment.GetEnvironmentVariable("ConnectionURI") ?? throw new ArgumentNullException("ConnectionURI environment variable not set"); 
});

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

// tilføjer Repository til services
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
