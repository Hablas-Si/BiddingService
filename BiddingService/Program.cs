using BiddingService.Repositories;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using Models;

var builder = WebApplication.CreateBuilder(args);

// BsonSeralizer... fortæller at hver gang den ser en Guid i alle entiteter skal den serializeres til en string. 
BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

// OBS: lig dem her op i vault, se opgave
string mySecret = Environment.GetEnvironmentVariable("Secret") ?? "none";
string myIssuer = Environment.GetEnvironmentVariable("Issuer") ?? "none";

builder.Services.Configure<MongoDBSettings>(options =>
{
    options.ConnectionURI = Environment.GetEnvironmentVariable("ConnectionURI") ?? throw new ArgumentNullException("ConnectionURI environment variable not set"); 
    options.DatabaseName = Environment.GetEnvironmentVariable("DatabaseName") ?? throw new ArgumentNullException("DatabaseName environment variable not set");
    options.CollectionName = Environment.GetEnvironmentVariable("CollectionName") ?? throw new ArgumentNullException("CollectionName environment variable not set");
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


app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();

app.Run();
