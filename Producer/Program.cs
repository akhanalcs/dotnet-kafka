using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes; //Serdes means Serializer and Deserializer
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Populate config objects
builder.Services.Configure<ProducerConfig>(builder.Configuration.GetSection("KafkaProducer")); // OR var producerConfig = builder.Configuration.GetSection("KafkaProducer").Get<ProducerConfig>();
builder.Services.Configure<SchemaRegistryConfig>(builder.Configuration.GetSection("KafkaSchemaRegistry"));

const string biometricsImportedTopicName = "BiometricsImported";

// Producer
builder.Services.AddSingleton<IProducer<string, Biometrics>>(sp =>
{
    var config = sp.GetRequiredService<IOptions<ProducerConfig>>();
    var schemaRegistryClient = sp.GetRequiredService<ISchemaRegistryClient>();
    return new ProducerBuilder<string, Biometrics>(config.Value)
         // Provide a Serializer to the Producer
        .SetValueSerializer(new JsonSerializer<Biometrics>(schemaRegistryClient))
        .Build();
});

// Schema Registry
builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
{
    var config = sp.GetRequiredService<IOptions<SchemaRegistryConfig>>();
    return new CachedSchemaRegistryClient(config.Value);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Logger.LogInformation("Adding Routes");
app.MapPost("/biometrics", async (Biometrics metrics, IProducer<string, Biometrics> producer) =>
    {
        var message = new Message<string, Biometrics> { Key = metrics.DeviceId.ToString(), Value = metrics };
        var result = await producer.ProduceAsync(biometricsImportedTopicName, message);
        producer.Flush();
        app.Logger.LogInformation("Accepted Biometrics");
        return TypedResults.Accepted("", result.Value); // result.Value is just message.Value
    })
    .WithName("RecordMeasurements")
    .WithOpenApi();

app.Logger.LogInformation("Starting the app");
app.Run();

record Biometrics(Guid DeviceId, List<HeartRate> HeartRates);
record HeartRate(DateTime DateTime, int Value);